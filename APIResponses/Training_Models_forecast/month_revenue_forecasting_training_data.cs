using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Training_Models
{
    public class month_revenue_forecasting_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUE TO FORECAST 
        public decimal? total_revenue { get; set; }
        public int pharmacy_total_transactions { get; set; }
        public decimal? average_bill_amount { get; set; }

        //derived values     
        public decimal? average_pharmacy_sale_per_transaction { get; set; }
        public int total_patient { get; set; }
    }
}
