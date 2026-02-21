using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class monthPharmacySalesComparisonResponse
    {
        public int baseMonth { get; set; }
        public int baseYear { get; set; }
        public int? baseTotalTransactions { get; set; }
        public decimal? baseTotalSales { get; set; }
        public string? baseTopSellingItem { get; set; }


        public int partnerMonth { get; set; }
        public int partnerYear { get; set; }
        public int? partnerTotalTransactions { get; set; }
        public decimal? partnerTotalSales { get; set; }
        public string? partnerTopSellingItem { get; set; }
    }
}
