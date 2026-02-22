using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Startup_Project.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankSessionId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionPayments_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionPayments_ScheduledSessions_ScheduledSessionId",
                        column: x => x.ScheduledSessionId,
                        principalTable: "ScheduledSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionPayments_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_BankSessionId",
                table: "SessionPayments",
                column: "BankSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_OrderId",
                table: "SessionPayments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_ScheduledSessionId",
                table: "SessionPayments",
                column: "ScheduledSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_Status",
                table: "SessionPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_StudentId",
                table: "SessionPayments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_StudentId_ScheduledSessionId",
                table: "SessionPayments",
                columns: new[] { "StudentId", "ScheduledSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionPayments_TrainerId",
                table: "SessionPayments",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionPayments");
        }
    }
}
