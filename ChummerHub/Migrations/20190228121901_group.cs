/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group'
    public partial class group : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "GameMasterUsername",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MySettingsId",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SINnerGroupSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DownloadUrl = table.Column<string>(nullable: true),
                    GoogleDriveFileId = table.Column<string>(nullable: true),
                    MyGroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINnerGroupSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_MySettingsId",
                table: "SINnerGroups",
                column: "MySettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerGroups_SINnerGroupSettings_MySettingsId",
                table: "SINnerGroups",
                column: "MySettingsId",
                principalTable: "SINnerGroupSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'group.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'group.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerGroups_SINnerGroupSettings_MySettingsId",
                table: "SINnerGroups");

            migrationBuilder.DropTable(
                name: "SINnerGroupSettings");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_MySettingsId",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "GameMasterUsername",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MySettingsId",
                table: "SINnerGroups");
        }
    }
}
