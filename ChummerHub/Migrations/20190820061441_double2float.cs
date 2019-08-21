using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float'
    public partial class double2float : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float.Up(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueDouble",
                table: "Tags");

            migrationBuilder.AddColumn<float>(
                name: "TagValueFloat",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags",
                column: "TagValueFloat");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueFloat",
                table: "Tags");

            migrationBuilder.AddColumn<double>(
                name: "TagValueDouble",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags",
                column: "TagValueDouble");
        }
    }
}
