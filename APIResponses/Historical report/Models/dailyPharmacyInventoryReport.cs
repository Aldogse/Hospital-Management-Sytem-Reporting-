using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class dailyPharmacyInventoryReport
    {
        [Key]
        public int report_id { get; set; }
        public string med_name { get; set; }
        public int stock_quantity { get; set; }

        //will be used to inform about medicine that will expire soon 
        public DateOnly? expiry_date { get; set; }
        public string? status { get; set; }
        public DateTime reportDate { get; set; }
    }
}
