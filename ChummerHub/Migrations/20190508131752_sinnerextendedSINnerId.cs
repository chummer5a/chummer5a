using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId'
    public partial class sinnerextendedSINnerId : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId.Up(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINners_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropColumn(
                name: "MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.AddColumn<Guid>(
                name: "SINnerId",
                table: "SINnerExtendedMetaData",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINners_EditionNumber",
                table: "SINners",
                column: "EditionNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerExtendedMetaData_SINnerId",
                table: "SINnerExtendedMetaData",
                column: "SINnerId",
                unique: true,
                filter: "[SINnerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerExtendedMetaData_SINners_SINnerId",
                table: "SINnerExtendedMetaData",
                column: "SINnerId",
                principalTable: "SINners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'sinnerextendedSINnerId.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerExtendedMetaData_SINners_SINnerId",
                table: "SINnerExtendedMetaData");

            migrationBuilder.DropIndex(
                name: "IX_SINners_EditionNumber",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINnerExtendedMetaData_SINnerId",
                table: "SINnerExtendedMetaData");

            migrationBuilder.DropColumn(
                name: "SINnerId",
                table: "SINnerExtendedMetaData");

            migrationBuilder.AddColumn<Guid>(
                name: "MyExtendedAttributesId",
                table: "SINners",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINners_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtendedMetaData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
