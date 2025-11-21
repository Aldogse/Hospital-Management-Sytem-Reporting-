using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class daily_pharmacy_sales
    {
        [Key]
        public int report_id { get; set; }
        public int quantity_sold { get; set; }
        public decimal? total_amount { get; set; }
        public DateTime sale_date { get; set; }
    }
}
