using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerStudentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainerStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Program = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerStudents_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainerStudents_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainerStudents_StudentId",
                table: "TrainerStudents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerStudents_TrainerId",
                table: "TrainerStudents",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerStudents_TrainerId_StudentId",
                table: "TrainerStudents",
                columns: new[] { "TrainerId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainerStudents");
        }
    }
}
