using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.journal_responses
{
    public class pharmacyRangeQueryResponse
    {
        public int? totalTransactions { get; set; }
        public decimal? totalSales { get; set; }
        public string? topSellingItem { get; set; }
        public List<month_pharmacy_sales>months { get; set; }
    }
}
