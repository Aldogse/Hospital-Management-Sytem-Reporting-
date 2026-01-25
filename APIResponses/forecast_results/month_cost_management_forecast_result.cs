using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.forecast_results
{
    public class month_cost_management_forecast_result
    {
        [Key]
        public int result_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public float month_forecasted_cost { get; set; }
    }
}
