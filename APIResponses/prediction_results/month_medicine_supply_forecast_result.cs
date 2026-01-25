using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.prediction_results
{
    public class month_medicine_supply_forecast_result
    {
        [Key]
        public int training_id { get; set; }
        public int med_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal? avg_daily_use { get; set; }

        //data to be predicted
        public bool? shortage_occured { get; set; }
    }
}

