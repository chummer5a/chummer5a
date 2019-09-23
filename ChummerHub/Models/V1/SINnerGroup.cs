using ChummerHub.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Serialization;

namespace ChummerHub.Models.V1
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup'
    public class SINnerGroup
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup'
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Id'
        public Guid? Id { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Id'


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyParentGroupId'
        public Guid? MyParentGroupId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MyParentGroupId'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.IsPublic'
        public bool IsPublic { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.IsPublic'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GroupCreatorUserName'
        public string GroupCreatorUserName { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GroupCreatorUserName'

        [Obsolete]
        [NotMapped]
        [JsonIgnore]
        [XmlIgnore]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GameMasterUsername'
        public string GameMasterUsername { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GameMasterUsername'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MySettings'
        public SINnerGroupSetting MySettings { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.MySettings'

        [MaxLength(64)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Groupname'
        public string Groupname { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Groupname'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.PasswordHash'
        public string PasswordHash { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.PasswordHash'

        [NotMapped]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.HasPassword'
        public bool HasPassword { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.HasPassword'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Description'
        public string Description { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Description'

        [MaxLength(6)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Language'
        public string Language { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.Language'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.SINnerGroup()'
        public SINnerGroup()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.SINnerGroup()'
        {
            MyGroups = new List<SINnerGroup>();
            MySettings = new SINnerGroupSetting();
            HasPassword = false;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GetGroupMembers(ApplicationDbContext, bool)'
        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context, bool addTags)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerGroup.GetGroupMembers(ApplicationDbContext, bool)'
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
                                    .Include(a => a.SINnerMetaData.Visibility)
                                    .Include(a => a.SINnerMetaData.Visibility.UserRights)
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
                                                      .Include(a => a.SINnerMetaData)
                                                      //.Include(a => a.MyExtendedAttributes)
                                                      .Include(a => a.SINnerMetaData.Visibility)
                                                      .Include(a => a.SINnerMetaData.Visibility.UserRights)
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

