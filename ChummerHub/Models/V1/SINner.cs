//using Swashbuckle.AspNetCore.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Linq;
using Microsoft.AspNetCore;
using ChummerHub.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Models.V1
{
    [DebuggerDisplay("SINner {Id}")]
    public class SINner
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public Guid? Id { get; set; }

        
        public string DownloadUrl { get; set; }

        public DateTime? UploadDateTime { get; set; }

        public DateTime LastChange { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public Guid UploadClientId { get; set; }

        public SINnerMetaData SINnerMetaData { get; set; }

        public String JsonSummary { get; set; }

        public SINnerGroup MyGroup { get; set; }

        public string Alias { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string GoogleDriveFileId { get; set; }

        public SINner()
        {
            Id = Guid.NewGuid();
            this.SINnerMetaData = new SINnerMetaData();
        }

        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        private List<Tag> _AllTags { get; set; }

        //[JsonIgnore]
        //[XmlIgnore]
        //[NotMapped]
        //public List<Tag> AllTags
        //{
        //    get
        //    {
        //        if (_AllTags == null)
        //        {
        //            _AllTags = GetTagsForSinnerFlat(this.Id);
        //        }
        //        return _AllTags;
        //    }
        //    set
        //    {
        //        _AllTags = value;
        //    }
        //}

        public async Task<List<Tag>> GetTagsForSinnerFlat(ApplicationDbContext context)
        {
            return await (from a in context.Tags where a.SINnerId == this.Id select a).ToListAsync();
            
        }

        internal static async Task<List<SINner>> GetSINnersFromUser(ApplicationUser user, ApplicationDbContext context, bool canEdit)
        {
            List<SINner> result = new List<SINner>();
            var userseq = (from a in context.UserRights where a.EMail == user.NormalizedEmail && a.CanEdit == canEdit select a).ToList();
            foreach(var ur in userseq)
            {
                if(ur?.SINnerId == null) continue;
                var sin = await context.SINners.Include(a => a.SINnerMetaData.Visibility.UserRights)
                    .Include(b => b.MyGroup)
                    .FirstOrDefaultAsync(a => a.Id == ur.SINnerId);
                if(sin != null)
                {
                    result.Add(sin);
                }
            }
            return result;
        }
    }
}
