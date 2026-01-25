using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Pharmacy
{
    public class pharmacy_stock_batches
    {
        [Key]
        public int batch_id { get; set; }
        public int med_id { get; set; }
        public string? batch_no { get; set; }
        public int stock_quantity { get; set; }
        public DateOnly? expiry_date { get; set; }
        public DateTime date_added { get; set; }
    }
}
