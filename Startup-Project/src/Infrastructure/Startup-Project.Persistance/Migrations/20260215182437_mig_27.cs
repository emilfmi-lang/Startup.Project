using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class mig_27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GymId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExerciseType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PricePerPerson = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledSessions_AspNetUsers_MemberId",
                        column: x => x.MemberId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledSessions_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduledSessions_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_GymId",
                table: "ScheduledSessions",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_MemberId",
                table: "ScheduledSessions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_ScheduledTime",
                table: "ScheduledSessions",
                column: "ScheduledTime");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_Status",
                table: "ScheduledSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_TrainerId",
                table: "ScheduledSessions",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledSessions_TrainerId_ScheduledTime",
                table: "ScheduledSessions",
                columns: new[] { "TrainerId", "ScheduledTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledSessions");
        }
    }
}
