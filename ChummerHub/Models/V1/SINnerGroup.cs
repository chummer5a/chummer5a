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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINner.MyHash'
        public string MyHash
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINner.MyHash'
        {
            get
            {
                if (string.IsNullOrEmpty(Hash))
                    Hash = $"{Id.ToString().GetHashCode():X}";
                return Hash;
            }
            set { Hash = value; }
        }

        public SINnerGroup()
        {
            MyGroups = new List<SINnerGroup>();
            MySettings = new SINnerGroupSetting();
            HasPassword = PasswordHash?.Length > 0;
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
                        groupmembers = await context.SINners
                            //.Include(a => a.MyGroup)
                            .Include(a => a.SINnerMetaData)
                            //.Include(a => a.MyExtendedAttributes)
                            .Include(a => a.SINnerMetaData.Visibility)
                            .Include(a => a.SINnerMetaData.Visibility.UserRights)
                            .Where(a => a.MyGroup.Id == Id && Id != null).ToListAsync();
                    }

                    foreach (var member in groupmembers)
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
                    return groupmembers;
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message, e);
                    throw;
                }
            }

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyGroups'
        public List<SINnerGroup> MyGroups { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyGroups'

        [ForeignKey("MyParentGroupId")]
        [JsonIgnore]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyParentGroup'
        public SINnerGroup MyParentGroup { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyParentGroup'

        /// <summary>
        /// Only users of the specified Role can join this group
        /// </summary>
        [MaxLength(64)]
        public string MyAdminIdentityRole { get; set; }

    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting'
    public class SINnerGroupSetting : SINnerUploadAble
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting.Id'
#pragma warning disable CS0108 // 'SINnerGroupSetting.Id' hides inherited member 'SINnerUploadAble.Id'. Use the new keyword if hiding was intended.
        public Guid? Id { get; set; }
#pragma warning restore CS0108 // 'SINnerGroupSetting.Id' hides inherited member 'SINnerUploadAble.Id'. Use the new keyword if hiding was intended.
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting.Id'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting.MyGroupId'
        public Guid MyGroupId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroupSetting.MyGroupId'
    }
}

