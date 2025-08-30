using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.HR;

namespace APIResponses.Historical_report
{
    public class employeeReportFinalData
    {
        [Key]
        public int? reportId { get; set; }
        public int employeeId { get; set; }
        public string employeeName { get; set; }
        public DateOnly? payPeriodStartDate { get; set; }
        public decimal? overtimePay { get; set; }
        public decimal? overtimeHours { get; set; }
        public decimal? payCycleGrossPay { get; set; }
        public decimal? GrossPay { get; set; }
        public decimal? payCycleTotalDeductions { get; set; }
        public decimal? ytdTotalDeductions { get; set; }
        public decimal? ytdNetPay { get; set; }
        public decimal? payCycleNetpay { get; set; }
        public decimal? payCycleSssDeduction { get; set; }
        public decimal? ytdsssDeductions { get; set; }
        public decimal? payCyclePhilHealthDeduction { get; set; }
        public decimal? ytdphilHealthDeductions { get; set; }
        public decimal? payCycleLoanDeduction { get; set; }
        public decimal? ytdLoanDeductions { get; set; }
        public decimal? payCycleAbsenceDeduction { get; set; }
        public decimal? ytdAbsenceDeductions { get; set; }

        public decimal? payCyclePagibigDeductions { get; set; }
        public decimal? ytdPagibigDeductions { get; set; }
        public string? dateGenerated { get; set; }
    }
}
