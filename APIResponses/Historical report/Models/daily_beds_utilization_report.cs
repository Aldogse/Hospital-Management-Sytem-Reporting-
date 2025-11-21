using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_beds_utilization_report
    {
        [Key]
        public int report_id { get; set; }
        public int? bed_assigned { get; set; }
        public int? bed_released { get; set; }
        public int available_beds { get; set; }
        public int occupied_beds { get; set; }
        public DateTime? report_date { get; set; }
    }
}
