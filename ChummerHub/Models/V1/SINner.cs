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

        [JsonIgnore]
        [XmlIgnore]
        [NotMapped]
        public List<Tag> AllTags
        {
            get
            {
                if (_AllTags == null)
                {
                    _AllTags = GetTagsForSinnerFlat(this.Id);
                }
                return _AllTags;
            }
            set
            {
                _AllTags = value;
            }
        }

        private List<Tag> GetTagsForSinnerFlat(Guid? id)
        {
            using(var scope = Program.MyHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                Data.ApplicationDbContext context = services.GetRequiredService<Data.ApplicationDbContext>();
                return (from a in context.Tags where a.SINnerId == id select a).ToList();
            }
        }
    }
}
