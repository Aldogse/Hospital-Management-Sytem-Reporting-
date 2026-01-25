using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class yearRevenueResponse
    {
        public int month { get; set; }
        public int year { get; set; }
        public decimal? total_revenue { get; set; }
    }
}
