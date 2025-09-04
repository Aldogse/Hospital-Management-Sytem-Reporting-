using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report
{
    [Table("employeeannualpayrollreports")]
    public class employeeAnnualPayrollReport
    {
        [Key]
        public int reportId { get; set; }
        public int employeeId { get; set; }
        public decimal? yearTotalHoursWorked { get; set; }
        public decimal? yearTotalOvertimeHoursWorked { get; set; }
        public decimal? yearTotalWage { get; set; }
        public int year { get; set; }
    }
}
