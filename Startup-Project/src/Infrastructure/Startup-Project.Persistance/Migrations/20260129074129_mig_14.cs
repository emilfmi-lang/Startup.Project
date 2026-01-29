using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class mig_14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_Gyms_GymId",
                table: "Trainers");

            migrationBuilder.AlterColumn<Guid>(
                name: "GymId",
                table: "TrainingSessions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "GymId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnlineTrainer",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Trainers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaLinks",
                table: "Trainers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoIntroUrl",
                table: "Trainers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_IsOnlineTrainer",
                table: "Trainers",
                column: "IsOnlineTrainer");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_IsOnlineTrainer_Status",
                table: "Trainers",
                columns: new[] { "IsOnlineTrainer", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_IsVerified",
                table: "Trainers",
                column: "IsVerified");

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_Gyms_GymId",
                table: "Trainers",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_Gyms_GymId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_IsOnlineTrainer",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_IsOnlineTrainer_Status",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_IsVerified",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "IsOnlineTrainer",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "SocialMediaLinks",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "VideoIntroUrl",
                table: "Trainers");

            migrationBuilder.AlterColumn<Guid>(
                name: "GymId",
                table: "TrainingSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "GymId",
                table: "Trainers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_Gyms_GymId",
                table: "Trainers",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
