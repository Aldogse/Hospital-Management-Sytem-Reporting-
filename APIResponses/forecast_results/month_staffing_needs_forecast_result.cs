using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.forecast_results
{
    public class month_staffing_needs_forecast_result
    {
        [Key]
        public int result_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string department { get; set; }

        public decimal total_working_hours { get; set; }
        public int total_staff_needed { get; set; }
    }
}
