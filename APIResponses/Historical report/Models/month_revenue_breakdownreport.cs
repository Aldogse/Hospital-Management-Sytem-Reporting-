using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_revenue_breakdownreport
    {
        [Key]
        public int report_id { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public string description { get; set; }
        public decimal? amount { get; set; }
        public bool isStored { get; set; }
    }
}
