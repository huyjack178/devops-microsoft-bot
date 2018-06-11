using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Fanex.Bot.Migrations
{
    public partial class AddTimeToModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "MessageInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedTime",
                table: "MessageInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "LogInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedTime",
                table: "LogInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "GitLabInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedTime",
                table: "GitLabInfo",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "MessageInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedTime",
                table: "MessageInfo");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "LogInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedTime",
                table: "LogInfo");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "GitLabInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedTime",
                table: "GitLabInfo");
        }
    }
}
