using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class monthRevenueBreakdownResponse
    {
        public int report_id { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public string description { get; set; }
        public decimal? amount { get; set; }
    }
}
