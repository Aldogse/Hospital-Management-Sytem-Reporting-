using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.Billing
{
    public class billing_items
    {
        [Key]
        public int item_id { get; set; }


        [ForeignKey("Billing_Records")]
        public int? billing_id { get; set; }
        [JsonIgnore]
        public billing_records Billing_Records { get; set; }


        public itemType item_type { get; set; }
        public string item_description { get; set; }
        public int quantity { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? total_price { get; set; }
    }
}
