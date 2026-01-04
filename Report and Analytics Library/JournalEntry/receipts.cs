using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.JournalEntry
{
    public  class receipts
    {
        [Key]
        public int id { get; set; }
        public int order_id { get; set; }
        public int vendor_id { get; set; }
        public decimal subtotal { get; set; }
        public  decimal vat { get; set; }
        public decimal total { get; set; }
        public DateTime created_at { get; set; }
    }
}
