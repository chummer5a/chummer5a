using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class favGroupsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFavoriteGroup_FavoriteGuid",
                table: "ApplicationUserFavoriteGroup",
                column: "FavoriteGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationUserFavoriteGroup_FavoriteGuid",
                table: "ApplicationUserFavoriteGroup");
        }
    }
}
