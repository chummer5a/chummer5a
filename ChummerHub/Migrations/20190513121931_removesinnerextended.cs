using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class removesinnerextended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SINnerExtendedMetaData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
