using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class group : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
