using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _07 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChummerUploadClientId",
                table: "SINners");

            migrationBuilder.AddColumn<Guid>(
                name: "UploadClientId",
                table: "SINners",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SINners_UploadClientId",
                table: "SINners",
                column: "UploadClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SINners_UploadClientId",
                table: "SINners");

            migrationBuilder.DropColumn(
                name: "UploadClientId",
                table: "SINners");

            migrationBuilder.AddColumn<Guid>(
                name: "ChummerUploadClientId",
                table: "SINners",
                nullable: true);
        }
    }
}
