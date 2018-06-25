namespace Fanex.Bot.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageInfo",
                columns: table => new
                {
                    ConversationId = table.Column<string>(),
                    FromId = table.Column<string>(nullable: true),
                    FromName = table.Column<string>(nullable: true),
                    ToId = table.Column<string>(nullable: true),
                    ToName = table.Column<string>(nullable: true),
                    ServiceUrl = table.Column<string>(nullable: true),
                    ChannelId = table.Column<string>(nullable: true),
                    LogCategories = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(),
                    IsAdmin = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageInfo", x => x.ConversationId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageInfo");
        }
    }
}