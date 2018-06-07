using Microsoft.EntityFrameworkCore.Migrations;

namespace Fanex.Bot.Migrations
{
    public partial class AddGitLabInfoKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GitLabInfo",
                table: "GitLabInfo");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectUrl",
                table: "GitLabInfo",
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GitLabInfo",
                table: "GitLabInfo",
                columns: new[] { "ConversationId", "ProjectUrl" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GitLabInfo",
                table: "GitLabInfo");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectUrl",
                table: "GitLabInfo",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_GitLabInfo",
                table: "GitLabInfo",
                column: "ConversationId");
        }
    }
}