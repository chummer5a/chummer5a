using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups'
    public partial class favoritegroups : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_ApplicationUserId",
                table: "SINnerGroups",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerGroups_AspNetUsers_ApplicationUserId",
                table: "SINnerGroups",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favoritegroups.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerGroups_AspNetUsers_ApplicationUserId",
                table: "SINnerGroups");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_ApplicationUserId",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "SINnerGroups");
        }
    }
}
