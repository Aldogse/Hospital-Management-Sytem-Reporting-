using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Training_Models
{
    public class month_medicine_shortage_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int med_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int current_stock { get; set; }
        public decimal? avg_daily_use { get; set; }
        public int? total_dispensed_month  { get; set; }
        public bool expiring_within_30_days { get; set; }

        //data to be predicted
        public bool? shortage_occured { get; set; }
    }
}
