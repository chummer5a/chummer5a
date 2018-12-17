using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class reset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRights_Id",
                table: "UserRights");

            migrationBuilder.DropIndex(
                name: "IX_UploadClients_Id",
                table: "UploadClients");

            migrationBuilder.DropIndex(
                name: "IX_Tags_SINnerId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_SINnerVisibility_Id",
                table: "SINnerVisibility");

            migrationBuilder.DropIndex(
                name: "IX_SINnerComments_Id",
                table: "SINnerComments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRights_Id",
                table: "UserRights",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UploadClients_Id",
                table: "UploadClients",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SINnerId",
                table: "Tags",
                column: "SINnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerVisibility_Id",
                table: "SINnerVisibility",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerComments_Id",
                table: "SINnerComments",
                column: "Id");
        }
    }
}
