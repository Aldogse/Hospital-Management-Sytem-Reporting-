using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class pharmacy_inventory
    {
        [Key]
        public int med_id { get; set; }
        public string med_name { get; set; }
        public string category { get; set; }
        public int dosage { get; set; }
        public int stock_quantity { get; set; }
        public string? unit { get; set; }
        public DateOnly? expiry_date { get; set; }
        public int? supplier_id { get; set; }
        public string? status { get; set; }
    }
}
