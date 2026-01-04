using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_operational_records_report
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? total_operational_cost { get; set; }
        public decimal? total_gross_paid { get; set; }
        public decimal? total_disposed_medicine_cost { get; set; }
        public decimal? total_receipts_vendor_cost { get; set; }
        public DateTime created_at { get; set; }
    }
}
