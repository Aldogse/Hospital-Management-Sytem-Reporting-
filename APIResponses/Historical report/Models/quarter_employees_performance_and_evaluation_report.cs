using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class quarter_employees_performance_and_evaluation_report
    {
        [Key]
        public int reportId { get; set; }
        public string month { get; set; }
        public int year { get; set; }
        public int? totalEmployeesEvaluated { get; set; }
        public decimal? averagePerformanceScore { get; set; }
        public decimal? lowPerformers { get; set; }
    }
}
