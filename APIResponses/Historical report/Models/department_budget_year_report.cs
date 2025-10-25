using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class department_budget_year_report
    {
        [Key]
        public int report_id { get; set; }
        public int year { get; set; }
        public decimal? total_allocated { get; set; }
        public decimal? total_requested { get; set; }
        public decimal? total_approved { get; set; }
        public DateTime last_update_date { get; set; }
    }
}
