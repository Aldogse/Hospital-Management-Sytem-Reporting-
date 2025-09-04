using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report
{
    [Table("employeepayrollmonthreports")]
    public class employeePayrollMonthReport
    {
        [Key]
        public int reportId { get; set; }
        public int employeeId { get; set; }
        public decimal? monthOvertimeHours { get; set; }
        public decimal? monthTotalHoursWorked { get; set; }
        public decimal? monthTotalWage { get; set; }
        public int month { get; set; }
        public int year { get; set; }
    }
}
