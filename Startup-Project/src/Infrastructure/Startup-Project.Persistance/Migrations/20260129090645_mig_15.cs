using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class mig_15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs");

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "CheckInLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GymId",
                table: "CheckInLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CheckInLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TokenUsed",
                table: "CheckInLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_GymId",
                table: "CheckInLogs",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_Status",
                table: "CheckInLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs",
                columns: new[] { "UserId", "CheckInTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_UserId_Status",
                table: "CheckInLogs",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInLogs_Gyms_GymId",
                table: "CheckInLogs",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInLogs_Gyms_GymId",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_GymId",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_Status",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_UserId_Status",
                table: "CheckInLogs");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "CheckInLogs");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "CheckInLogs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CheckInLogs");

            migrationBuilder.DropColumn(
                name: "TokenUsed",
                table: "CheckInLogs");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs",
                columns: new[] { "UserId", "CheckInTime" });
        }
    }
}
