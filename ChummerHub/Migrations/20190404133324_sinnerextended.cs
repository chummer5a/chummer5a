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
