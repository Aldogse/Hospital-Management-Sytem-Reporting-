using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_revenue_report_forecast_entity
    {
        public float month { get; set; }
        public float year { get; set; }

        //TARGET VALUE TO FORECAST 
        public float total_revenue { get; set; }
        public float pharmacy_total_transactions { get; set; }
        public float average_pharmacy_sale_per_transaction { get; set; }

        //derived values
        public float average_bill_amount { get; set; }
        public float total_patient { get; set; }
    }
}
