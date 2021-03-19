using Microsoft.EntityFrameworkCore.Migrations;

namespace Blogifier.Core.Data.Migrations
{
    public partial class LogEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogEvent",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogEvent",
                table: "Logs");
        }
    }
}
