using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddGymIdSubscriptionIdToCheckInToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GymId",
                table: "CheckInTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "CheckInTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "CheckInLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInTokens_GymId",
                table: "CheckInTokens",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInTokens_SubscriptionId",
                table: "CheckInTokens",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInTokens_UserId_GymId_IsUsed",
                table: "CheckInTokens",
                columns: new[] { "UserId", "GymId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_SubscriptionId",
                table: "CheckInLogs",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInLogs_Subscriptions_SubscriptionId",
                table: "CheckInLogs",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInTokens_Gyms_GymId",
                table: "CheckInTokens",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckInTokens_Subscriptions_SubscriptionId",
                table: "CheckInTokens",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckInLogs_Subscriptions_SubscriptionId",
                table: "CheckInLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckInTokens_Gyms_GymId",
                table: "CheckInTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckInTokens_Subscriptions_SubscriptionId",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInTokens_GymId",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInTokens_SubscriptionId",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInTokens_UserId_GymId_IsUsed",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_SubscriptionId",
                table: "CheckInLogs");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "CheckInTokens");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "CheckInTokens");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "CheckInLogs");
        }
    }
}
