using Microsoft.EntityFrameworkCore.Migrations;

namespace Fanex.Bot.Migrations
{
    public partial class AddLogIgnoreMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogIgnoreMessage",
                columns: table => new
                {
                    Category = table.Column<string>(),
                    IgnoreMessage = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogIgnoreMessage", x => new { x.Category, x.IgnoreMessage });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogIgnoreMessage");
        }
    }
}