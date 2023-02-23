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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2'
    public partial class alias2 : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2.Up(MigrationBuilder)'
        protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2.Up(MigrationBuilder)'
        {
            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRights_EMail",
                table: "UserRights",
                column: "EMail");

            migrationBuilder.CreateIndex(
                name: "IX_SINners_Alias",
                table: "SINners",
                column: "Alias");

            migrationBuilder.CreateIndex(
                name: "IX_SINnerGroups_Groupname",
                table: "SINnerGroups",
                column: "Groupname");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'alias2.Down(MigrationBuilder)'
        protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'alias2.Down(MigrationBuilder)'
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRights_EMail",
                table: "UserRights");

            migrationBuilder.DropIndex(
                name: "IX_SINners_Alias",
                table: "SINners");

            migrationBuilder.DropIndex(
                name: "IX_SINnerGroups_Groupname",
                table: "SINnerGroups");

            migrationBuilder.AlterColumn<string>(
                name: "EMail",
                table: "UserRights",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Alias",
                table: "SINners",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Groupname",
                table: "SINnerGroups",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
