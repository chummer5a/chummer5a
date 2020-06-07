using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble'
    public partial class TagValueDouble : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<double>(
                name: "TagValueDouble",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags",
                column: "TagValueDouble");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TagValueDouble.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueDouble",
                table: "Tags");
        }
    }
}
