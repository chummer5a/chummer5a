using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _08 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChummerUploadClients",
                table: "ChummerUploadClients");

            migrationBuilder.RenameTable(
                name: "ChummerUploadClients",
                newName: "UploadClients");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UploadClients",
                table: "UploadClients",
                column: "UploadClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UploadClients",
                table: "UploadClients");

            migrationBuilder.RenameTable(
                name: "UploadClients",
                newName: "ChummerUploadClients");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChummerUploadClients",
                table: "ChummerUploadClients",
                column: "UploadClientId");
        }
    }
}
