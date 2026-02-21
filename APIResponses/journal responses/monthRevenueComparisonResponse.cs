using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.journal_responses
{
    public class monthRevenueComparisonResponse
    {
        public int baseMonth { get; set; }
        public int baseYear { get; set; }
        public decimal? baseServiceRevenue { get; set; }
        public decimal? basePharmacyRevenue { get; set; }
        public decimal? baseTotalRevenue { get; set; }

        public int partnerMonth { get; set; }
        public int partnerYear { get; set; }
        public decimal? partnerServiceRevenue { get; set; }
        public decimal? partnerPharmacyRevenue { get; set; }
        public decimal? partnerTotalRevenue { get; set; }
    }
}
