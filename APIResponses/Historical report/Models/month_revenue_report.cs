using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_revenue_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? service_revenue { get; set; }
        public decimal? pharmacy_revenue { get; set; }
        public decimal? total_revenue { get; set; }
        public DateTime last_update_date { get; set; }
    }
}
