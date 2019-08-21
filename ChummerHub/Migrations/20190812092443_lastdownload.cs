using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'lastdownload'
    public partial class lastdownload : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'lastdownload'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'lastdownload.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'lastdownload.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastDownload",
                table: "SINners",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'lastdownload.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'lastdownload.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "LastDownload",
                table: "SINners");
        }
    }
}
