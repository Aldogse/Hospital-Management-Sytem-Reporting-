using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.BillingResponse
{
    public class monthPharmacySalesReportResponse
    {
        public int totalTransactions { get; set; }
        public decimal? totalSales { get; set; }
        public string topSellingItem { get; set; }
        public List<monthPharmacySalesDetailsResponse> items { get; set; }
    }
}
