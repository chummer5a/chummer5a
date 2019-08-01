using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class favoritegroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
