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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups'
    public partial class recursiveGroups : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Up(MigrationBuilder)'
        {
            migrationBuilder.AddColumn<string>(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MyParentGroupId",
                table: "SINnerGroups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups",
                column: "MyParentGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SINnerGroups_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups",
                column: "MyParentGroupId",
                principalTable: "SINnerGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'recursiveGroups.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINnerGroups_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_MyParentGroupId",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MyAdminIdentityRole",
                table: "SINnerGroups");

            migrationBuilder.DropColumn(
                name: "MyParentGroupId",
                table: "SINnerGroups");
        }
    }
}
