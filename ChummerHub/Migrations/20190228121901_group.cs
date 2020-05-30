using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group'
    public partial class group : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "GameMasterUsername",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MySettingsId",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SINnerGroupSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DownloadUrl = table.Column<string>(nullable: true),
                    GoogleDriveFileId = table.Column<string>(nullable: true),
                    MyGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerGroupSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_MySettingsId",
                table: "SINnerGroups",
                column: "MySettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerGroups_SINnerGroupSettings_MySettingsId",
                table: "SINnerGroups",
                column: "MySettingsId",
                principalTable: "SINnerGroupSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerGroups_SINnerGroupSettings_MySettingsId",
                table: "SINnerGroups");

            migrationBuilder.DropTable(
                name: "SINnerGroupSettings");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_MySettingsId",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "GameMasterUsername",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MySettingsId",
                table: "SINnerGroups");
        }
    }
}
