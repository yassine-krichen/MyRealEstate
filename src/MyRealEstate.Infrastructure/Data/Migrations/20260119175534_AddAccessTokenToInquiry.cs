using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyRealEstate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessTokenToInquiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Inquiries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Inquiries");
        }
    }
}
