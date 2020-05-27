using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias'
    public partial class alias : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias.Down(MigrationBuilder)'
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "SINners");
        }
    }
}
