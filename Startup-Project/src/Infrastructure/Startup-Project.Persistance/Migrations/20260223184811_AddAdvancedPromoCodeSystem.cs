using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedPromoCodeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Packages_PackageId",
                table: "PromoCodes");

            migrationBuilder.AlterColumn<Guid>(
                name: "PackageId",
                table: "PromoCodeUsages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduledSessionId",
                table: "PromoCodeUsages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "PromoCodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "PromoCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainerId",
                table: "PromoCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_CreatedById",
                table: "PromoCodes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_TrainerId",
                table: "PromoCodes",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_AspNetUsers_CreatedById",
                table: "PromoCodes",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Packages_PackageId",
                table: "PromoCodes",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Trainers_TrainerId",
                table: "PromoCodes",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_AspNetUsers_CreatedById",
                table: "PromoCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Packages_PackageId",
                table: "PromoCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Trainers_TrainerId",
                table: "PromoCodes");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodes_CreatedById",
                table: "PromoCodes");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodes_TrainerId",
                table: "PromoCodes");

            migrationBuilder.DropColumn(
                name: "ScheduledSessionId",
                table: "PromoCodeUsages");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "PromoCodes");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "PromoCodes");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "PromoCodes");

            migrationBuilder.AlterColumn<Guid>(
                name: "PackageId",
                table: "PromoCodeUsages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Packages_PackageId",
                table: "PromoCodes",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
