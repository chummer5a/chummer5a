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

        [MaxLength(6)]
        public string Language { get; set; }

        public SINnerGroup()
        {
            MyGroups = new List<SINnerGroup>();
        }

        public async Task<List<SINner>> GetGroupMembers(ApplicationDbContext context)
        {
            try
            {
                var groupmembers = await (from a in context.SINners
                            .Include(a => a.MyGroup)
                            .Include(a => a.SINnerMetaData)
                            .Include(a => a.SINnerMetaData.Visibility)
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

        [ForeignKey("MyParentGroupId")]
        public SINnerGroup MyParentGroup { get; set; }

        /// <summary>
        /// Only users of the specified Role can join this group
        /// </summary>
        [MaxLength(64)]
        public string MyAdminIdentityRole { get; set; }
        
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

