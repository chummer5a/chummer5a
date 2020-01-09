using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended'
    public partial class removesinnerextended : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended.Up(MigrationBuilder)'
        {
            migrationBuilder.DropTable(
                name: "SINnerExtendedMetaData");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'removesinnerextended.Down(MigrationBuilder)'
        {
            migrationBuilder.CreateTable(
                name: "SINnerExtendedMetaData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    JsonSummary = table.Column<string>(nullable: true),
                    SINnerId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerExtendedMetaData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SINnerExtendedMetaData_SINners_SINnerId",
                        column: x => x.SINnerId,
                        principalTable: "SINners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SINnerExtendedMetaData_SINnerId",
                table: "SINnerExtendedMetaData",
                column: "SINnerId",
                unique: true,
                filter: "[SINnerId] IS NOT NULL");
        }
    }
}
