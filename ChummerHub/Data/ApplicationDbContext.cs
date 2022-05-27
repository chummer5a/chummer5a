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
using Microsoft.Extensions.Hosting;
using Duende.IdentityServer.EntityFramework.Interfaces;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore.Design;

namespace ChummerHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>//, IPersistedGrantDbContext
    {
        public IHostEnvironment HostingEnvironment { get; set; }
        public IConfiguration Configuration { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHostEnvironment env)
            : base(options)
        {
            HostingEnvironment = env;
        }

        public ApplicationDbContext()
        {

        }

        public override int SaveChanges()
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
                string wholeMessage = "Error while validating Entities:" + Environment.NewLine;
                foreach (var valResult in validationResults)
                {
                    string msg = "Members " + string.Join(", ", valResult.MemberNames) + " not valid: ";
                    msg += valResult.ErrorMessage;
                    wholeMessage += msg + Environment.NewLine;
                }
                var ex = new HubException(wholeMessage);
                foreach (var valResult in validationResults)
                {
                    foreach (var member in valResult.MemberNames)
                    {
                        ex.Data.Add("member_" + member, valResult.ErrorMessage);
                    }
                }
                throw ex;
            }

            return base.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
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



        protected override void OnModelCreating(ModelBuilder builder)
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
            
        }

        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        #region models
        public DbSet<Models.V1.SINner> SINners { get; set; }

        public DbSet<Models.V1.SINnerGroup> SINnerGroups { get; set; }

        public DbSet<Models.V1.Tag> Tags { get; set; }

        public DbSet<Models.V1.SINnerUserRight> UserRights { get; set; }

        public DbSet<Models.V1.UploadClient> UploadClients { get; set; }

        //public DbSet<ChummerHub.Models.V1.SINnerExtended> SINnerExtendedMetaData { get; set; }

        public DbSet<Models.V1.SINnerComment> SINnerComments { get; set; }
        public DbSet<Models.V1.SINnerVisibility> SINnerVisibility { get; set; }
        public DbSet<Models.V1.SINnerMetaData> SINnerMetaData { get; set; }

        public DbSet<Models.V1.SINnerGroupSetting> SINnerGroupSettings { get; set; }

        #endregion

        //public DbSet<PersistedGrant> PersistedGrants { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public DbSet<DeviceFlowCodes> DeviceFlowCodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public DbSet<Key> Keys { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            string constring = "Server=(localdb)\\mssqllocaldb;Database=SINners_DB;Trusted_Connection=True;MultipleActiveResultSets=true";
            optionsBuilder.UseSqlServer(constring);
            
            return new ApplicationDbContext();
        }
    }
}
