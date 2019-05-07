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
using System.Transactions;
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

        [MaxLength(6)]
        public string Language { get; set; }

        public SINnerMetaData SINnerMetaData { get; set; }
        
        public SINnerExtended MyExtendedAttributes { get; set; }

        public SINnerGroup MyGroup { get; set; }

        [MaxLength(64)]
        public string Alias { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string GoogleDriveFileId { get; set; }

        public SINner()
        {
            Id = Guid.NewGuid();
            this.SINnerMetaData = new SINnerMetaData();
            this.MyExtendedAttributes = new SINnerExtended();
            this.DownloadUrl = "";
            this.MyGroup = null;
            this.Language = "";
        }

        internal static async Task<List<SINner>> GetSINnersFromUser(ApplicationUser user, ApplicationDbContext context, bool canEdit)
        {
            using (var t = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }, TransactionScopeAsyncFlowOption.Enabled))
            {
                List<SINner> result = new List<SINner>();
                var userseq = await (from a in context.UserRights
                    where a.EMail == user.NormalizedEmail && a.CanEdit == canEdit
                    select a.SINnerId).ToListAsync();
                var sinseq = await context.SINners
                    .Include(a => a.MyGroup)
                    .Where(a => userseq.Contains(a.Id)).ToListAsync();
                t.Complete();
                return sinseq;
            }
        }
    }
}
