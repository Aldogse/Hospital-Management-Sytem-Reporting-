using APIResponses.Historical_report;
using Microsoft.EntityFrameworkCore;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_API.Data
{
    public class HistoricalReportDbContext: DbContext
    {
        public HistoricalReportDbContext(DbContextOptions<HistoricalReportDbContext> options) : base(options)
        {
            
        }

        public DbSet<employeeReportFinalData> payrollInformation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<employeeReportFinalData>(entity =>
            {
                entity.Property(e => e.overtimePay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleGrossPay).HasPrecision(18, 2);
                entity.Property(e => e.GrossPay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleTotalDeductions).HasPrecision(18, 2);
                entity.Property(e => e.ytdTotalDeductions).HasPrecision(18, 2);
                entity.Property(e => e.ytdNetPay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleNetpay).HasPrecision(18, 2);
                entity.Property(e => e.payCycleSssDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdsssDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCyclePhilHealthDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdphilHealthDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCycleLoanDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdLoanDeductions).HasPrecision(18, 2);
                entity.Property(e => e.payCycleAbsenceDeduction).HasPrecision(18, 2);
                entity.Property(e => e.ytdAbsenceDeductions).HasPrecision(18, 2);
            });
        }

    }
}
