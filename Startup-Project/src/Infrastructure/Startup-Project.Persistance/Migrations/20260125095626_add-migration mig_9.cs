using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class addmigrationmig_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GymReview_AspNetUsers_AppUserId",
                table: "GymReview");

            migrationBuilder.DropForeignKey(
                name: "FK_GymReview_AspNetUsers_UserId",
                table: "GymReview");

            migrationBuilder.DropForeignKey(
                name: "FK_GymReview_Gyms_GymId",
                table: "GymReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GymReview",
                table: "GymReview");

            migrationBuilder.RenameTable(
                name: "GymReview",
                newName: "GymReviews");

            migrationBuilder.RenameIndex(
                name: "IX_GymReview_UserId",
                table: "GymReviews",
                newName: "IX_GymReviews_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GymReview_GymId_UserId",
                table: "GymReviews",
                newName: "IX_GymReviews_GymId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GymReview_AppUserId",
                table: "GymReviews",
                newName: "IX_GymReviews_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GymReviews",
                table: "GymReviews",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GymOpeningHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    GymId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymOpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymOpeningHours_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "FK_GymReviews_AspNetUsers_UserId",
                table: "GymReviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GymReviews_Gyms_GymId",
                table: "GymReviews",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GymReviews_AspNetUsers_AppUserId",
                table: "GymReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GymReviews_AspNetUsers_UserId",
                table: "GymReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GymReviews_Gyms_GymId",
                table: "GymReviews");

            migrationBuilder.DropTable(
                name: "GymOpeningHours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GymReviews",
                table: "GymReviews");

            migrationBuilder.RenameTable(
                name: "GymReviews",
                newName: "GymReview");

            migrationBuilder.RenameIndex(
                name: "IX_GymReviews_UserId",
                table: "GymReview",
                newName: "IX_GymReview_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GymReviews_GymId_UserId",
                table: "GymReview",
                newName: "IX_GymReview_GymId_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GymReviews_AppUserId",
                table: "GymReview",
                newName: "IX_GymReview_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GymReview",
                table: "GymReview",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GymReview_AspNetUsers_AppUserId",
                table: "GymReview",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GymReview_AspNetUsers_UserId",
                table: "GymReview",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GymReview_Gyms_GymId",
                table: "GymReview",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
