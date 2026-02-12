using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.journal_responses
{
    public class yearRevenueBreakdownResponse
    {
        public int year { get; set; }
        public decimal? yearTotalRevenue { get; set; }
        public decimal? serviceRevenue { get; set; }
        public decimal? pharmacy_revenue { get; set; }
        public List<month_revenue_report> monthsRevenue { get; set; }
    }
}
