using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class visibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId",
                table: "UploadClients",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SINnerVisibility",
                columns: table => new
                {
                    SINnerVisibilityId = table.Column<Guid>(nullable: false),
                    IsPublic = table.Column<bool>(nullable: false),
                    IsGroupVisible = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerVisibility", x => x.SINnerVisibilityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadClients_SINnerVisibilityId",
                table: "UploadClients",
                column: "SINnerVisibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerMetaData_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                column: "VisibilitySINnerVisibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId",
                table: "AspNetUsers",
                column: "SINnerVisibilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId",
                table: "AspNetUsers",
                column: "SINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData",
                column: "VisibilitySINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UploadClients_SINnerVisibility_SINnerVisibilityId",
                table: "UploadClients",
                column: "SINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_SINnerMetaData_SINnerVisibility_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadClients_SINnerVisibility_SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropTable(
                name: "SINnerVisibility");

            migrationBuilder.DropIndex(
                name: "IX_UploadClients_SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropIndex(
                name: "IX_SINnerMetaData_VisibilitySINnerVisibilityId",
                table: "SINnerMetaData");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropColumn(
                name: "VisibilitySINnerVisibilityId",
                table: "SINnerMetaData");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId",
                table: "AspNetUsers");
        }
    }
}
