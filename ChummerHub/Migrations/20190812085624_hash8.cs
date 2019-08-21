using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'hash8'
    public partial class hash8 : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'hash8'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'hash8.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'hash8.Up(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "SINners",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'hash8.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'hash8.Down(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "Hash",
                table: "SINners",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 8,
                oldNullable: true);
        }
    }
}
