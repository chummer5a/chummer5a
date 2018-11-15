using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UploadClients",
                schema: "SINner",
                newName: "UploadClients");

            migrationBuilder.RenameTable(
                name: "Tag",
                schema: "SINner",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "SINners",
                schema: "SINner",
                newName: "SINners");

            migrationBuilder.RenameTable(
                name: "SINnerMetaData",
                schema: "SINner",
                newName: "SINnerMetaData");

            migrationBuilder.RenameTable(
                name: "SINnerComments",
                schema: "SINner",
                newName: "SINnerComments");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "SINner",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "SINner",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "SINner",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "SINner",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "SINner",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "SINner",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "SINner",
                newName: "AspNetRoleClaims");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SINner");

            migrationBuilder.RenameTable(
                name: "UploadClients",
                newName: "UploadClients",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tag",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "SINners",
                newName: "SINners",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "SINnerMetaData",
                newName: "SINnerMetaData",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "SINnerComments",
                newName: "SINnerComments",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "SINner");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "SINner");
        }
    }
}
