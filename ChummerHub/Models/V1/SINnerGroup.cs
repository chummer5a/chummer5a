/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Serialization;
using ChummerHub.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChummerHub.Models.V1
{

    public class SINnerGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }
        public Guid? MyParentGroupId { get; set; }
        public bool IsPublic { get; set; }
        public string GroupCreatorUserName { get; set; }

        [Obsolete]
        [NotMapped]
        [JsonIgnore]
        [XmlIgnore]
        public string GameMasterUsername { get; set; }

        public SINnerGroupSetting MySettings { get; set; }

        [MaxLength(64)]
        public string Groupname { get; set; }
        public string PasswordHash { get; set; }

        [NotMapped]
        public bool HasPassword { get; set; }

        public string Description { get; set; }

        [MaxLength(6)]
        public string Language { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [MaxLength(8)]
        internal string Hash { get; set; }

        [NotMapped]
        [MaxLength(8)]
        public string MyHash
        {
            get
            {
                if (string.IsNullOrEmpty(Hash))
                    Hash = $"{Id.ToString().GetHashCode():X}";
                return Hash;
            }
            set => Hash = value;
        }

        public SINnerGroup()
        {
            MyGroups = new List<SINnerGroup>();
            MySettings = new SINnerGroupSetting();
            HasPassword = PasswordHash?.Length > 0;
            this.Id = Guid.Empty;
        }

        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context, bool addTags)
        {
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadUncommitted

                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    List<SINner> groupmembers;
                    /*
                    = await (context.SINners.Include(a => a.MyGroup)
                        .Include(a => a.SINnerMetaData)
                        .Include(a => a.SINnerMetaData.Visibility)
                        .Where(a => a.MyGroup.Id == Id && Id != null
                                                       && (a.SINnerMetaData.Visibility.IsGroupVisible
                                                           || a.SINnerMetaData.Visibility.IsPublic)).ToListAsync();
                    */
                    if (addTags)
                    {
                        groupmembers = await context.SINners
                            .Include(a => a.MyGroup)
                            .Include(a => a.SINnerMetaData)
                            .Include(a => a.SINnerMetaData.Visibility)
                            .Include(a => a.SINnerMetaData.Visibility.UserRights)
                            .Include(a => a.SINnerMetaData.Tags)
                            .ThenInclude(b => b.Tags)
                            .ThenInclude(b => b.Tags)
                            .ThenInclude(b => b.Tags)
                            .ThenInclude(b => b.Tags)
                            .ThenInclude(b => b.Tags)
                            .Where(a => a.MyGroup.Id == Id && Id != null).ToListAsync();
                    }
                    else
                    {
                        try
                        {
                            groupmembers = await context.SINners
                            .Include(a => a.MyGroup)
                            .Include(a => a.SINnerMetaData)
                            .Include(a => a.SINnerMetaData.Visibility)
                            .Include(a => a.SINnerMetaData.Visibility.UserRights)
                            .Where(a => a.MyGroup.Id == Id && Id != null).ToListAsync();
                        }
                        catch(InvalidOperationException e)
                        {
                            groupmembers = await context.SINners
                            .Include(a => a.SINnerMetaData)
                            .Include(a => a.SINnerMetaData.Visibility)
                            .Include(a => a.SINnerMetaData.Visibility.UserRights)
                            .Where(a => a.MyGroup.Id == Id && Id != null).ToListAsync();
                        }
                        
                    }

                    foreach (var member in groupmembers)
                    {
                        if (member.SINnerMetaData == null)
                            member.SINnerMetaData = new SINnerMetaData();
                        if (member.SINnerMetaData.Tags == null)
                            member.SINnerMetaData.Tags = new List<Tag>();
                        if (member.SINnerMetaData.Visibility == null)
                            member.SINnerMetaData.Visibility = new SINnerVisibility();
                        if (member.SINnerMetaData.Visibility.UserRights == null)
                            member.SINnerMetaData.Visibility.UserRights = new List<SINnerUserRight>();
                    }
                    t.Complete();
                    return groupmembers;
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message, e);
                    throw;
                }
            }

        }

        public List<SINnerGroup> MyGroups { get; set; }

        [ForeignKey("MyParentGroupId")]
        [JsonIgnore]
        public SINnerGroup MyParentGroup { get; set; }

        /// <summary>
        /// Only users of the specified Role can join this group
        /// </summary>
        [MaxLength(64)]
        public string MyAdminIdentityRole { get; set; }

    }

    public class SINnerGroupSetting : SINnerUploadAble
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public Guid? Id { get; set; }

        public Guid MyGroupId { get; set; }
    }
}

