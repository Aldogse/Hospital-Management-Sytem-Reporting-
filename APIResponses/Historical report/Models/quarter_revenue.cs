using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class quarter_revenue
    {
        [Key]
        public int reportId { get; set; }
        public int year { get; set; }
        public int quarter { get; set; }
        public decimal? totalRevenue { get; set; }
    }
}
