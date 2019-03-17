using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class recursiveGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MyParentGroupId",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups",
                column: "MyParentGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerGroups_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups",
                column: "MyParentGroupId",
                principalTable: "SINnerGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerGroups_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MyParentGroupId",
                table: "SINnerGroups");
        }
    }
}
