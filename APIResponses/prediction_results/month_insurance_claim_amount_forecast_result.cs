using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.prediction_results
{
    public class month_insurance_claim_amount_forecast_result
    {
        [Key]
        public int result_id { get; set; }
        public int insurance_provider_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUES
        public decimal total_claim_approved_amount { get; set; }
        public decimal total_claim_declined_amount { get; set; }
    }
}
