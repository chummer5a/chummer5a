using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
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

        public bool IsPublic { get; set; }

        public string GameMasterUsername { get; set; }

        public SINnerGroupSetting MySettings { get; set; }


        public string Groupname { get; set; }

        public SINnerGroup()
        {
            //MySINners = new List<SINner>();
            MyGroups = new List<SINnerGroup>();
        }

        //public List<SINner> MySINners { get; set; }

        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context)
        {
            try
            {
                var groupmembers = await (from a in context.SINners
                                   where a.MyGroup.Id == this.Id
                                         && this.Id != null
                                         && ((a.SINnerMetaData.Visibility.IsGroupVisible == true)
                                         || (a.SINnerMetaData.Visibility.IsPublic == true))
                                   select a).ToListAsync();
                return groupmembers;

            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.TraceError(e.Message, e);
                throw;
            }
        }

        public List<SINnerGroup> MyGroups { get; set; }

        public SINnerGroup MyParentGroup { get; set; }

        /// <summary>
        /// Only users of the specified Role can join this group
        /// </summary>
        public string MyAdminIdentityRole { get; set; }

        //public async Task<List<SINerUserRight>> GetUserRights(ApplicationDbContext context)
        //{
        //    List<SINerUserRight> result = new List<SINerUserRight>();
        //    try
        //    {
        //        var sinners = await this.GetSinners(context);

        //        foreach(var sinner in sinners)
        //        {
        //            var members = (from a in sinner.SINnerMetaData.Visibility.UserRights select a);
        //            foreach(var member in members)
        //            {
        //                if(!result.Contains(member))
        //                    result.Add(member);
        //            }
        //        }
        //        return result;
        //    }
        //    catch(Exception e)
        //    {
        //        System.Diagnostics.Trace.TraceError(e.Message, e);
        //        throw;
        //    }
        //}
    }

    public class SINnerGroupSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid? Id { get; set; }

        public string DownloadUrl { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string GoogleDriveFileId { get; set; }

        public Guid MyGroupId { get; set; }
    }
}

