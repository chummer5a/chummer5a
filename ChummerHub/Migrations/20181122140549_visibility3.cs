using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class visibility3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Groupname",
                table: "SINnerVisibility",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId1",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId1",
                table: "AspNetUsers",
                column: "SINnerVisibilityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId1",
                table: "AspNetUsers",
                column: "SINnerVisibilityId1",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Groupname",
                table: "SINnerVisibility");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId1",
                table: "AspNetUsers");
        }
    }
}
