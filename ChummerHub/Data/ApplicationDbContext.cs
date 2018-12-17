using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Identity;

namespace ChummerHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ChummerHub.Models.V1.SINner>()
                .HasIndex(b => b.Id).IsUnique();
            builder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => b.Id).IsUnique();
            builder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => new { b.TagName, b.TagValue });
            builder.Entity<ChummerHub.Models.V1.SINerUserRight>()
                .HasIndex(b => b.SINnerId);


        }

        public DbSet<ChummerHub.Models.V1.SINner> SINners { get; set; }

        public DbSet<ChummerHub.Models.V1.Tag> Tags { get; set; }

        public DbSet<ChummerHub.Models.V1.SINerUserRight> UserRights { get; set; }

        public DbSet<ChummerHub.Models.V1.UploadClient> UploadClients { get; set; }

        public DbSet<ChummerHub.Models.V1.SINnerComment> SINnerComments { get; set; }
        public DbSet<ChummerHub.Models.V1.SINnerVisibility> SINnerVisibility { get; set; }
        public DbSet<ChummerHub.Models.V1.SINnerMetaData> SINnerMetaData { get; set; }





    }
}
