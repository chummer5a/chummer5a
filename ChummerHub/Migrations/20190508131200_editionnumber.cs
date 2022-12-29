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

namespace ChummerHub.Migrations
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber'
    public partial class editionnumber : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Up(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerExtended",
                table: "SINnerExtended");

            migrationBuilder.RenameTable(
                name: "SINnerExtended",
                newName: "SINnerExtendedMetaData");

            migrationBuilder.AddColumn<string>(
                name: "EditionNumber",
                table: "SINners",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerExtendedMetaData",
                table: "SINnerExtendedMetaData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtendedMetaData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'editionnumber.Down(MigrationBuilder)'
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SINners_SINnerExtendedMetaData_MyExtendedAttributesId",
                table: "SINners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SINnerExtendedMetaData",
                table: "SINnerExtendedMetaData");

            migrationBuilder.DropColumn(
                name: "EditionNumber",
                table: "SINners");

            migrationBuilder.RenameTable(
                name: "SINnerExtendedMetaData",
                newName: "SINnerExtended");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SINnerExtended",
                table: "SINnerExtended",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SINners_SINnerExtended_MyExtendedAttributesId",
                table: "SINners",
                column: "MyExtendedAttributesId",
                principalTable: "SINnerExtended",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
