using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Billing
{
    public class bill_items
    {
        [Key]
        public int item_id { get; set; }

        [ForeignKey("bills")]
        public int bill_id { get; set; }
        public bills bills { get; set; }


        [ForeignKey("biiling_Service")]
        public int service_id { get; set; }
        public biiling_service biiling_Service { get; set; }


        public int quantity { get; set; }
        public decimal unit_price { get; set; }
        public decimal total_price { get; set; }
    }
}
