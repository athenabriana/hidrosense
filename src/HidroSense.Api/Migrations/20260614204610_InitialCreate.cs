using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HidroSense.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumerUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IdleFlowThresholdLpm = table.Column<double>(type: "float", nullable: false),
                    MonthlyBudgetLiters = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerUnitId = table.Column<int>(type: "int", nullable: false),
                    ReadingTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VolumeLiters = table.Column<double>(type: "float", nullable: false),
                    FlowRateLpm = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumptionReadings_ConsumerUnits_ConsumerUnitId",
                        column: x => x.ConsumerUnitId,
                        principalTable: "ConsumerUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeakAlerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerUnitId = table.Column<int>(type: "int", nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TriggeringFlowRateLpm = table.Column<double>(type: "float", nullable: false),
                    EstimatedWasteLiters = table.Column<double>(type: "float", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeakAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeakAlerts_ConsumerUnits_ConsumerUnitId",
                        column: x => x.ConsumerUnitId,
                        principalTable: "ConsumerUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerUnits_Code",
                table: "ConsumerUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerUnits_Region",
                table: "ConsumerUnits",
                column: "Region");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionReadings_ConsumerUnitId_ReadingTimeUtc",
                table: "ConsumptionReadings",
                columns: new[] { "ConsumerUnitId", "ReadingTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionReadings_ReadingTimeUtc",
                table: "ConsumptionReadings",
                column: "ReadingTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LeakAlerts_ConsumerUnitId",
                table: "LeakAlerts",
                column: "ConsumerUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LeakAlerts_Status",
                table: "LeakAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeakAlerts_Status_Severity",
                table: "LeakAlerts",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumptionReadings");

            migrationBuilder.DropTable(
                name: "LeakAlerts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ConsumerUnits");
        }
    }
}
