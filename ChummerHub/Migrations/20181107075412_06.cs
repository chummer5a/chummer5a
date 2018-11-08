using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class _06 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_ChummerUploadClient_ChummerUploadClientUploadClientId",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINners_ChummerUploadClientUploadClientId",
                table: "SINners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerComment",
                table: "SINnerComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChummerUploadClient",
                table: "ChummerUploadClient");

            migrationBuilder.RenameTable(
                name: "SINnerComment",
                newName: "SINnerComments");

            migrationBuilder.RenameTable(
                name: "ChummerUploadClient",
                newName: "ChummerUploadClients");

            migrationBuilder.RenameColumn(
                name: "DownloadUrl",
                table: "SINners",
                newName: "DownloadUrlInternal");

            migrationBuilder.RenameColumn(
                name: "ChummerUploadClientUploadClientId",
                table: "SINners",
                newName: "ChummerUploadClientId");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerComment_SINnerId",
                table: "SINnerComments",
                newName: "IX_SINnerComments_SINnerId");

            migrationBuilder.AddColumn<int>(
                name: "Downloads",
                table: "SINnerComments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerComments",
                table: "SINnerComments",
                column: "SINnerCommentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChummerUploadClients",
                table: "ChummerUploadClients",
                column: "UploadClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerComments",
                table: "SINnerComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChummerUploadClients",
                table: "ChummerUploadClients");

            migrationBuilder.DropColumn(
                name: "Downloads",
                table: "SINnerComments");

            migrationBuilder.RenameTable(
                name: "SINnerComments",
                newName: "SINnerComment");

            migrationBuilder.RenameTable(
                name: "ChummerUploadClients",
                newName: "ChummerUploadClient");

            migrationBuilder.RenameColumn(
                name: "DownloadUrlInternal",
                table: "SINners",
                newName: "DownloadUrl");

            migrationBuilder.RenameColumn(
                name: "ChummerUploadClientId",
                table: "SINners",
                newName: "ChummerUploadClientUploadClientId");

            migrationBuilder.RenameIndex(
                name: "IX_SINnerComments_SINnerId",
                table: "SINnerComment",
                newName: "IX_SINnerComment_SINnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerComment",
                table: "SINnerComment",
                column: "SINnerCommentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChummerUploadClient",
                table: "ChummerUploadClient",
                column: "UploadClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_ChummerUploadClientUploadClientId",
                table: "SINners",
                column: "ChummerUploadClientUploadClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_ChummerUploadClient_ChummerUploadClientUploadClientId",
                table: "SINners",
                column: "ChummerUploadClientUploadClientId",
                principalTable: "ChummerUploadClient",
                principalColumn: "UploadClientId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
