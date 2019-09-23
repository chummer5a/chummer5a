﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration'
    public partial class cumulativeMigration : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration.Up(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagValue",
                table: "Tags",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagComment",
                table: "Tags",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "SINners",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "SINnerGroups",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_Language",
                table: "SINnerGroups",
                column: "Language");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'cumulativeMigration.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_Language",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "SINners");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "SINnerGroups");

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagValue",
                table: "Tags",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagName",
                table: "Tags",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TagComment",
                table: "Tags",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
