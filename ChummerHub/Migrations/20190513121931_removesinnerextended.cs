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
