using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable'
    public partial class sinneruploadable : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastChange",
                table: "SINnerGroupSettings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDateTime",
                table: "SINnerGroupSettings",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinneruploadable.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "LastChange",
                table: "SINnerGroupSettings");

            migrationBuilder.DropColumn(
                name: "UploadDateTime",
                table: "SINnerGroupSettings");
        }
    }
}
