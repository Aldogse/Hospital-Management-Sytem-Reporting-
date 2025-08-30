using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Report_and_Analytics_API.Data.Migrations.Historical
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payrollInformation",
                columns: table => new
                {
                    reportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    employeeName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payPeriodStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    overtimePay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    overtimeHours = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    payCycleGrossPay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GrossPay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCycleTotalDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdTotalDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdNetPay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCycleNetpay = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCycleSssDeduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdsssDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCyclePhilHealthDeduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdphilHealthDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCycleLoanDeduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdLoanDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCycleAbsenceDeduction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ytdAbsenceDeductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    payCyclePagibigDeductions = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ytdPagibigDeductions = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    dateGenerated = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrollInformation", x => x.reportId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payrollInformation");
        }
    }
}
