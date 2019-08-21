using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids'
    public partial class favgroupsAreGuids : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids.Up(MigrationBuilder)'
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

            migrationBuilder.CreateTable(
                name: "ApplicationUserFavoriteGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FavoriteGuid = table.Column<Guid>(nullable: false),
                    ApplicationUserId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserFavoriteGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFavoriteGroup_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFavoriteGroup_ApplicationUserId",
                table: "ApplicationUserFavoriteGroup",
                column: "ApplicationUserId");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'favgroupsAreGuids.Down(MigrationBuilder)'
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserFavoriteGroup");

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
    }
}
