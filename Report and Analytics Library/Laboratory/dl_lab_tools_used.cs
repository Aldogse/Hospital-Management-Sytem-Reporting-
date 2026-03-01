using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Laboratory
{
    public class dl_lab_tools_used
    {
        [Key]
        public int id { get; set; }
        public int scheduleID { get; set; }
        public int patientID { get; set; }
        public int item_id { get; set; }
        public string? item_type { get; set; }
        public string? item_name { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public DateTime created_at { get; set; }
    }
}
