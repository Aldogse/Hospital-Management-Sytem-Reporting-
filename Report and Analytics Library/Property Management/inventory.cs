using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Property_Management
{
    public class inventory
    {
        [Key]
        public int id { get; set; }
        public int item_id { get; set; }
        public string item_name { get; set; }
        public string item_type { get; set; }
        public string? category { get; set; }
        public string? sub_type { get; set; }
        public int quantity { get; set; }
        public int total_qty { get; set; }
        public decimal price { get; set; }
        public string unit_type { get; set; }
        public int? pcs_per_box { get; set; }
        public DateTime received_at { get; set; }
        public string location { get; set; }
        public int min_stock { get; set; }
        public int max_stock { get; set; }

    }
}
