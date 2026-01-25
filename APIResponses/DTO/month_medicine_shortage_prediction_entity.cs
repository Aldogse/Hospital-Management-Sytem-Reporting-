using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_medicine_shortage_prediction_entity
    {
        public float training_id { get; set; }
        public float med_id { get; set; }
        public float month { get; set; }
        public float year { get; set; }
        public float current_stock { get; set; }
        public float avg_daily_use { get; set; }
        public float total_dispensed_month { get; set; }
        public bool expiring_within_30_days { get; set; }

        //data to be predicted
        public bool shortage_occured { get; set; }
    }
}
