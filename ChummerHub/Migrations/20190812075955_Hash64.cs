using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Hash64'
    public partial class Hash64 : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Hash64'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Hash64.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Hash64.Up(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "SINners",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINners_Hash",
                table: "SINners",
                column: "Hash");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Hash64.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Hash64.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_SINners_Hash",
                table: "SINners");

            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
