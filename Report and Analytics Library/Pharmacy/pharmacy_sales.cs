using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class pharmacy_sales
    {
        [Key]
        public int sale_id { get; set; }

        public string customer_name { get; set; }
        public int quantity_sold { get; set; }
        public decimal price_per_unit { get; set; }
        public decimal? total_price { get; set; }
        public DateTime sale_date { get; set; }
        public string? payment_method { get; set; }
        public string staff_name { get; set; }
    }
}
