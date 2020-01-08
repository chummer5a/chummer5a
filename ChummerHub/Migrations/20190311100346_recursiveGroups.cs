using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups'
    public partial class recursiveGroups : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Up(MigrationBuilder)'
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Down(MigrationBuilder)'
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
