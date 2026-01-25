using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.training_models_prediction
{
    public class month_insurance_claim_amount_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int insurance_provider_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUES
        public decimal total_claim_amount_submitted { get; set; }
        public decimal total_claim_approved_amount { get; set; }
        public decimal total_claim_declined_amount { get; set; }

        public decimal last_month_total_claim_approved_amount { get; set; }
        public decimal last_month_total_claim_declined_amount { get; set; }
    }
}
