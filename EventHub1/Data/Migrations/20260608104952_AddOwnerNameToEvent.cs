using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub1.Data.Migrations
{
    public partial class AddOwnerNameToEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Events");
        }
    }
}
