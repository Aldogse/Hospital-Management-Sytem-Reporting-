using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Insurance
{
    public class insurance_logs
    {
        [Key]
        public int log_id { get; set; }
        public int request_id { get; set; }
        public int patient_id { get; set; }
        public int billing_id { get; set; }
        public string? status { get; set; }
        public DateOnly date_transact { get; set; }
    }
}
