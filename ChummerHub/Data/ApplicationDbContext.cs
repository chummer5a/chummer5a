using ChummerHub.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ChummerHub.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext'
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.HostingEnvironment'
        public IHostingEnvironment HostingEnvironment { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.HostingEnvironment'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.Configuration'
        public IConfiguration Configuration { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.Configuration'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.ApplicationDbContext(DbContextOptions<ApplicationDbContext>, IHostingEnvironment)'
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHostingEnvironment env)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.ApplicationDbContext(DbContextOptions<ApplicationDbContext>, IHostingEnvironment)'
            : base(options)
        {
            HostingEnvironment = env;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.ApplicationDbContext()'
        public ApplicationDbContext()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.ApplicationDbContext()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SaveChanges()'
        public override int SaveChanges()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SaveChanges()'
        {
            bool error = false;
            Collection<ValidationResult> validationResults = new Collection<ValidationResult>();
            foreach (var entity in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity))
            {
                var validationContext = new ValidationContext(entity);
                //Validator.ValidateObject(entity, validationContext);
                if (!Validator.TryValidateObject(entity, validationContext, validationResults, true))
                {
                    error = true;
                }
            }
            if (error)
            {
                int counter = 0;
                string wholeMessage = "Error while validating Entities:" + Environment.NewLine;
                foreach (var valResult in validationResults)
                {
                    counter++;
                    string msg = "Members " + string.Join(", ", valResult.MemberNames) + " not valid: ";
                    msg += valResult.ErrorMessage;
                    wholeMessage += msg + Environment.NewLine;
                }
                var ex = new HubException(wholeMessage);
                counter = 0;
                foreach (var valResult in validationResults)
                {
                    counter++;
                    foreach (var member in valResult.MemberNames)
                    {
                        ex.Data.Add("member_" + member, valResult.ErrorMessage);
                    }
                }
                throw ex;
            }

            return base.SaveChanges();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.OnConfiguring(DbContextOptionsBuilder)'
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.OnConfiguring(DbContextOptionsBuilder)'
        {

            if (!optionsBuilder.IsConfigured)
            {

                if (HostingEnvironment == null)
                {
                    string constring = "Server=(localdb)\\mssqllocaldb;Database=SINners_DB;Trusted_Connection=True;MultipleActiveResultSets=true";
                    optionsBuilder.UseSqlServer(constring);

                    //throw new ArgumentNullException("HostingEnviroment is null!");
                }
                else
                {
                    var configurationBuilder = new ConfigurationBuilder()
                        .SetBasePath(HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    Configuration = configurationBuilder.Build();
                    optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                }
            }

            optionsBuilder.EnableSensitiveDataLogging();
        }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.OnModelCreating(ModelBuilder)'
        protected override void OnModelCreating(ModelBuilder builder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.OnModelCreating(ModelBuilder)'
        {
            base.OnModelCreating(builder);
            builder.Entity<Models.V1.Tag>()
                .HasIndex(b => new { b.TagName, b.TagValue });
            builder.Entity<Models.V1.Tag>()
                .HasIndex(b => b.SINnerId);
            builder.Entity<Models.V1.SINnerUserRight>()
                .HasIndex(b => b.SINnerId);
            builder.Entity<Models.V1.SINner>()
                .HasIndex(b => b.Alias);
            builder.Entity<Models.V1.SINner>()
                .HasIndex(b => b.Hash);
            builder.Entity<Models.V1.SINnerGroup>()
                .HasIndex(b => b.Groupname);
            builder.Entity<Models.V1.SINnerGroup>()
                .HasIndex(b => b.Hash);
            builder.Entity<Models.V1.SINnerUserRight>()
                .HasIndex(b => b.EMail);
            builder.Entity<Models.V1.SINnerGroup>()
                .HasIndex(b => b.Language);
            builder.Entity<Models.V1.SINner>()
                .HasIndex(b => b.EditionNumber);
            //builder.Entity<ChummerHub.Models.V1.SINnerExtended>()
            //    .HasIndex(b => b.SINnerId);
            builder.Entity<Models.V1.Tag>()
                .HasIndex(b => b.TagValueFloat);
            builder.Entity<ApplicationUserFavoriteGroup>()
                .HasIndex(b => b.FavoriteGuid);
            try
            {
                Database.ExecuteSqlCommand(
                    @"CREATE VIEW View_SINnerUserRights AS 
        SELECT        dbo.SINners.Alias, dbo.UserRights.EMail, dbo.SINners.Id, dbo.UserRights.CanEdit, dbo.SINners.GoogleDriveFileId, dbo.SINners.MyGroupId, dbo.SINners.LastChange
                         
FROM            dbo.SINners INNER JOIN
                         dbo.SINnerMetaData ON dbo.SINners.SINnerMetaDataId = dbo.SINnerMetaData.Id INNER JOIN
                         dbo.SINnerVisibility ON dbo.SINnerMetaData.VisibilityId = dbo.SINnerVisibility.Id INNER JOIN
                         dbo.UserRights ON dbo.SINnerVisibility.Id = dbo.UserRights.SINnerVisibilityId"
                );
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.TraceInformation(e.Message);
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINners'
        public DbSet<Models.V1.SINner> SINners { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINners'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerGroups'
        public DbSet<Models.V1.SINnerGroup> SINnerGroups { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerGroups'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.Tags'
        public DbSet<Models.V1.Tag> Tags { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.Tags'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.UserRights'
        public DbSet<Models.V1.SINnerUserRight> UserRights { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.UserRights'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.UploadClients'
        public DbSet<Models.V1.UploadClient> UploadClients { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.UploadClients'

        //public DbSet<ChummerHub.Models.V1.SINnerExtended> SINnerExtendedMetaData { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerComments'
        public DbSet<Models.V1.SINnerComment> SINnerComments { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerComments'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerVisibility'
        public DbSet<Models.V1.SINnerVisibility> SINnerVisibility { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerVisibility'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerMetaData'
        public DbSet<Models.V1.SINnerMetaData> SINnerMetaData { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerMetaData'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerGroupSettings'
        public DbSet<Models.V1.SINnerGroupSetting> SINnerGroupSettings { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ApplicationDbContext.SINnerGroupSettings'
    }
}
