using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'installationid'
    public partial class installationid : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'installationid'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'installationid.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'installationid.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstallationId",
                table: "UploadClients",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'installationid.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'installationid.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "InstallationId",
                table: "UploadClients");
        }
    }
}
