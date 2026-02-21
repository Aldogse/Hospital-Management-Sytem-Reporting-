using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class yearly_billing_report
    {
        [Key]
        public int report_id { get; set; }
        public int year { get; set; }
        public decimal? total_billed { get; set; }
        public decimal? total_paid { get; set; }
        public int? total_pending_transaction { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }
        public decimal? total_pending_amount { get; set; }
        public int lastBillingRecordId { get; set; }
    }
}
