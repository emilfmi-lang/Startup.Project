using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class mig_16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsOtps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsOtps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsOtps_PhoneNumber",
                table: "SmsOtps",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SmsOtps_PhoneNumber_Code",
                table: "SmsOtps",
                columns: new[] { "PhoneNumber", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsOtps_PhoneNumber_IsUsed",
                table: "SmsOtps",
                columns: new[] { "PhoneNumber", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsOtps_PhoneNumber_IsUsed_ExpiryDate",
                table: "SmsOtps",
                columns: new[] { "PhoneNumber", "IsUsed", "ExpiryDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsOtps");
        }
    }
}
