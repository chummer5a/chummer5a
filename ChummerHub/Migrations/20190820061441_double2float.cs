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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float'
    public partial class double2float : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float.Up(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueDouble",
                table: "Tags");

            migrationBuilder.AddColumn<float>(
                name: "TagValueFloat",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags",
                column: "TagValueFloat");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'double2float.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'double2float.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_TagValueFloat",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "TagValueFloat",
                table: "Tags");

            migrationBuilder.AddColumn<double>(
                name: "TagValueDouble",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagValueDouble",
                table: "Tags",
                column: "TagValueDouble");
        }
    }
}
