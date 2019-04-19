using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
    public partial class sinnerextended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonSummary",
                table: "SINners");

            migrationBuilder.AddColumn<Guid>(
                name: "MyExtendedAttributesId",
                table: "SINners",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SINnerExtended",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    JsonSummary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerExtended", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SINners_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtended",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropTable(
                name: "SINnerExtended");

            migrationBuilder.DropIndex(
                name: "IX_SINners_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropColumn(
                name: "MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.AddColumn<string>(
                name: "JsonSummary",
                table: "SINners",
                nullable: true);
        }
    }
}
