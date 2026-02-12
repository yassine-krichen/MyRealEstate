using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyRealEstate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDealExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosedAt",
                table: "Deals",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                table: "Deals",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "Deals",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InquiryId",
                table: "Deals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Deals",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deals_InquiryId",
                table: "Deals",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Status",
                table: "Deals",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Inquiries_InquiryId",
                table: "Deals",
                column: "InquiryId",
                principalTable: "Inquiries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Inquiries_InquiryId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_InquiryId",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_Status",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "InquiryId",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Deals");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClosedAt",
                table: "Deals",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
