using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class visibility4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId1",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadClients_SINnerVisibility_SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropIndex(
                name: "IX_UploadClients_SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId",
                table: "UploadClients");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SINnerVisibilityId1",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId",
                table: "UploadClients",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SINnerVisibilityId1",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadClients_SINnerVisibilityId",
                table: "UploadClients",
                column: "SINnerVisibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId",
                table: "AspNetUsers",
                column: "SINnerVisibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SINnerVisibilityId1",
                table: "AspNetUsers",
                column: "SINnerVisibilityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId",
                table: "AspNetUsers",
                column: "SINnerVisibilityId",
                principalTable: "SINnerVisibility",
                principalColumn: "SINnerVisibilityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SINnerVisibility_SINnerVisibilityId1",
                table: "AspNetUsers",
                column: "SINnerVisibilityId1",
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
    }
}
