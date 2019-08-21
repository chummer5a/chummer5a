using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended'
    public partial class sinnerextended : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended.Up(MigrationBuilder)'
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextended.Down(MigrationBuilder)'
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
