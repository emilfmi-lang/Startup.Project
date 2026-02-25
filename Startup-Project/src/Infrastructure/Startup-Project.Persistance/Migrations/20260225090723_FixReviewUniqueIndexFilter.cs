using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewUniqueIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainerReviews_TrainerId_UserId",
                table: "TrainerReviews");

            migrationBuilder.DropIndex(
                name: "IX_GymReviews_GymId_UserId",
                table: "GymReviews");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerReviews_TrainerId_UserId",
                table: "TrainerReviews",
                columns: new[] { "TrainerId", "UserId" },
                unique: true,
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GymReviews_GymId_UserId",
                table: "GymReviews",
                columns: new[] { "GymId", "UserId" },
                unique: true,
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainerReviews_TrainerId_UserId",
                table: "TrainerReviews");

            migrationBuilder.DropIndex(
                name: "IX_GymReviews_GymId_UserId",
                table: "GymReviews");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerReviews_TrainerId_UserId",
                table: "TrainerReviews",
                columns: new[] { "TrainerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymReviews_GymId_UserId",
                table: "GymReviews",
                columns: new[] { "GymId", "UserId" },
                unique: true);
        }
    }
}
