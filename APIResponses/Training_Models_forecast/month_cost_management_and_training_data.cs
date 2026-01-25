using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Training_Models
{
    public class month_cost_management_and_training_data
    {
        [Key]
        public int training_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUE
        public float total_month_operational_cost { get; set; }
        public float previous_month_operational_cost { get; set; }
        public float last_three_months_cost { get; set; }
        public float last_six_months_cost { get; set; }
    }
}
