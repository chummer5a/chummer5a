using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Serialization;
using ChummerHub.Data;
using Microsoft.AspNetCore.Identity;
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

        public SINnerGroup()
        {
            MyGroups = new List<SINnerGroup>();
            MySettings = new SINnerGroupSetting();
            HasPassword = false;
        }

        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context, bool addTags)
        {
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted

                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    {
                        List<SINner> groupmembers = null;
                        /*await (from a in context.SINners
                                .Include(a => a.MyGroup)
                                .Include(a => a.SINnerMetaData)
                                .Include(a => a.SINnerMetaData.Visibility)
                                            where a.MyGroup.Id == this.Id
                                                  && this.Id != null
                                                  && ((a.SINnerMetaData.Visibility.IsGroupVisible == true)
                                                      || (a.SINnerMetaData.Visibility.IsPublic == true))
                                            select a).ToListAsync();
                        */
                        if (addTags == true)
                        {
                            groupmembers = await (from a in context.SINners
                                    .Include(a => a.MyGroup)
                                    .Include(a => a.SINnerMetaData)
                                    .Include(a => a.SINnerMetaData.Tags)
                                    .ThenInclude(b => b.Tags)
                                    .ThenInclude(b => b.Tags)
                                    .ThenInclude(b => b.Tags)
                                    .ThenInclude(b => b.Tags)
                                    .ThenInclude(b => b.Tags)
                                where a.MyGroup.Id == this.Id
                                      && this.Id != null
                                select a).ToListAsync();
                        }
                        else
                        {
                            groupmembers = await (from a in context.SINners
                                //.Include(a => a.MyGroup)
                                //.Include(a => a.SINnerMetaData)
                                //.Include(a => a.MyExtendedAttributes)
                                //.Include(a => a.SINnerMetaData.Visibility)
                                where a.MyGroup.Id == this.Id
                                && this.Id != null
                            select a).ToListAsync();
                        }

                        var res = groupmembers;
                        foreach (var member in res)
                        {
                            //if (member.MyExtendedAttributes == null)
                            //    member.MyExtendedAttributes = new SINnerExtended(member);
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
                        return res;

                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.TraceError(e.Message, e);
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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        public Guid MyGroupId { get; set; }
    }
}

