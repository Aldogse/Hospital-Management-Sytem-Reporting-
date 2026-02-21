using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.BillingResponse
{
    public class monthBillingReportComparisonResponse
    {
        public int month { get; set; }
        public int year { get; set; }
        public decimal? total_billed { get; set; }
        public decimal? total_paid { get; set; }
        public int? total_pending_transaction { get; set; }
        public decimal? total_oop_collected { get; set; }
        public decimal? total_insurance_covered { get; set; }
        public decimal? total_pending_amount { get; set; }

        public int partnermonth { get; set; }
        public int partneryear { get; set; }
        public decimal? partnertotal_billed { get; set; }
        public decimal? partnertotal_paid { get; set; }
        public int? partnertotal_pending_transaction { get; set; }
        public decimal? partnertotal_oop_collected { get; set; }
        public decimal? partnertotal_insurance_covered { get; set; }
        public decimal? partnertotal_pending_amount { get; set; }
    }
}
