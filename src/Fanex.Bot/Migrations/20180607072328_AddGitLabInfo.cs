using Microsoft.EntityFrameworkCore.Migrations;

namespace Fanex.Bot.Migrations
{
    public partial class AddGitLabInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MessageInfo");

            migrationBuilder.DropColumn(
                name: "LogCategories",
                table: "MessageInfo");

            migrationBuilder.CreateTable(
                name: "GitLabInfo",
                columns: table => new
                {
                    ConversationId = table.Column<string>(),
                    ProjectUrl = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitLabInfo", x => x.ConversationId);
                });

            migrationBuilder.CreateTable(
                name: "LogInfo",
                columns: table => new
                {
                    ConversationId = table.Column<string>(),
                    LogCategories = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogInfo", x => x.ConversationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GitLabInfo");

            migrationBuilder.DropTable(
                name: "LogInfo");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MessageInfo",
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogCategories",
                table: "MessageInfo",
                nullable: true);
        }
    }
}