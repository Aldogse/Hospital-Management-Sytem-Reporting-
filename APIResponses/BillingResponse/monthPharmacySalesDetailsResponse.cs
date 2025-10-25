using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.BillingResponse
{
    public class monthPharmacySalesDetailsResponse
    {
        public int itemId { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public string paymentMethod { get; set; }
        public decimal? totalAmount { get; set; }
        public DateTime billingDate { get; set; }
    }
}
