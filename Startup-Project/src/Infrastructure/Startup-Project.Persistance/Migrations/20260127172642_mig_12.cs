using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class mig_12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GymReviews_AspNetUsers_AppUserId",
                table: "GymReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Packages_PackageId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_GymReviews_AppUserId",
                table: "GymReviews");

            migrationBuilder.DropIndex(
                name: "IX_GymOpeningHours_GymId",
                table: "GymOpeningHours");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "GymReviews");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Packages",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "ReviewCount",
                table: "Gyms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Gyms",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Gyms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AverageRating",
                table: "Gyms",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<bool>(
                name: "IsClosed",
                table: "GymOpeningHours",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CheckInTokens",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "CheckInTokens",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AuthProvider",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_EndDate",
                table: "Subscriptions",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_EndDate",
                table: "Subscriptions",
                columns: new[] { "UserId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IsDeleted",
                table: "Packages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_IsDeleted",
                table: "Gyms",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_Name",
                table: "Gyms",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_GymReviews_GymId",
                table: "GymReviews",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_GymOpeningHours_GymId_DayOfWeek",
                table: "GymOpeningHours",
                columns: new[] { "GymId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInTokens_ExpiryDate",
                table: "CheckInTokens",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInTokens_Token",
                table: "CheckInTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_CheckInTime",
                table: "CheckInLogs",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs",
                columns: new[] { "UserId", "CheckInTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GoogleId",
                table: "AspNetUsers",
                column: "GoogleId",
                unique: true,
                filter: "[GoogleId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Packages_PackageId",
                table: "Payments",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Packages_PackageId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_EndDate",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId_EndDate",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Packages_IsDeleted",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Gyms_IsDeleted",
                table: "Gyms");

            migrationBuilder.DropIndex(
                name: "IX_Gyms_Name",
                table: "Gyms");

            migrationBuilder.DropIndex(
                name: "IX_GymReviews_GymId",
                table: "GymReviews");

            migrationBuilder.DropIndex(
                name: "IX_GymOpeningHours_GymId_DayOfWeek",
                table: "GymOpeningHours");

            migrationBuilder.DropIndex(
                name: "IX_CheckInTokens_ExpiryDate",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInTokens_Token",
                table: "CheckInTokens");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_CheckInTime",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_CheckInLogs_UserId_CheckInTime",
                table: "CheckInLogs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GoogleId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Packages",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewCount",
                table: "Gyms",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Gyms",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "Gyms",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "AverageRating",
                table: "Gyms",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "GymReviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsClosed",
                table: "GymOpeningHours",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "CheckInTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "CheckInTokens",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AuthProvider",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GymReviews_AppUserId",
                table: "GymReviews",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GymOpeningHours_GymId",
                table: "GymOpeningHours",
                column: "GymId");

            migrationBuilder.AddForeignKey(
                name: "FK_GymReviews_AspNetUsers_AppUserId",
                table: "GymReviews",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Packages_PackageId",
                table: "Payments",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
