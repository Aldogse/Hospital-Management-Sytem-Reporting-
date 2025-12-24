using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class monthBillingReportResponse
    {
        public int month { get; set; }
        public int year { get; set; }
        public decimal? totalPaid { get; set; }
        public decimal? totalBilled { get; set; }
    }
}
