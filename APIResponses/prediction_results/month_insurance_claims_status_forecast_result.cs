using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.prediction_results
{
    public class month_insurance_claims_status_forecast_result
    {
        [Key]
        public int result_id { get; set; }
        public int insurance_provider_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        public int total_claims { get; set; }


        public int total_claim_approved { get; set; }
        public int total_claim_denied { get; set; }
    }
}
