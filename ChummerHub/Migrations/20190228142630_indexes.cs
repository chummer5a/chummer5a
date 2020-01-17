using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'indexes'
    public partial class indexes : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'indexes'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'indexes.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'indexes.Up(MigrationBuilder)'
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tags_SINnerId",
                table: "Tags",
                column: "SINnerId");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'indexes.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'indexes.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_SINnerId",
                table: "Tags");
        }
    }
}
