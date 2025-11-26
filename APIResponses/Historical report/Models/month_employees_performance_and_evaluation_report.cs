using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_employees_performance_and_evaluation_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int? total_evaluations { get; set; }
        public decimal? average_score { get; set; }
        public int? poor_performer_count { get; set; }
    }
}
