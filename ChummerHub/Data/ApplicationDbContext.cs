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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ChummerHub.Models.V1.SINner>()
                .HasIndex(b => b.SINnerId).IsUnique();
            modelBuilder.Entity<ChummerHub.Models.V1.SINnerComment>()
                .HasIndex(b => b.SINnerId);
            modelBuilder.Entity<ChummerHub.Models.V1.SINner>()
                .HasIndex(b => b.UploadClientId);
            modelBuilder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => b.TagId).IsUnique();
            modelBuilder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => b.SINnerId);
            modelBuilder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => new { b.TagName, b.TagValue });
            
        }

        public DbSet<ChummerHub.Models.V1.SINner> SINners { get; set; }

        public DbSet<ChummerHub.Models.V1.UploadClient> UploadClients { get; set; }

        public DbSet<ChummerHub.Models.V1.SINnerComment> SINnerComments { get; set; }

       

        

    }
}
