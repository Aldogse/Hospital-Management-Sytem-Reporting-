using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class employerAnnualPayrollReportBreakdown
    {
        [Key]
        public int reportId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? monthOvertimeHours { get; set; }
        public decimal? monthTotalHoursWorked { get; set; }
        public decimal? monthTotalWage { get; set; }
        public decimal? totalOvertimeHours { get; set; }
        public decimal? totalHoursWorked { get; set; }
        public decimal? totalWage { get; set; }
    }
}
