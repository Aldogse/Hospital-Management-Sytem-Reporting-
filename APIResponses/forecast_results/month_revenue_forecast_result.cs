using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.forecast_results
{
    public class month_revenue_forecast_result
    {
        [Key]
        public int training_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUE TO FORECAST 
        public decimal? total_revenue { get; set; }
        public int pharmacy_total_transactions { get; set; }
        public decimal? average_bill_amount { get; set; }
    }
}
