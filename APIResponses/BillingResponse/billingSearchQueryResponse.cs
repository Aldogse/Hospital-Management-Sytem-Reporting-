using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.BillingResponse
{
    public class billingSearchQueryResponse
    {
        public decimal? total_billed { get; set; }
        public decimal? total_paid { get; set; }
        public int? total_pending_transaction { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }
        public decimal? total_pending_amount { get; set; }
        public List<month_billing_report> months { get; set; }
    }
}
