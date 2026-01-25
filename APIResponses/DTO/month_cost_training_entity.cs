using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_cost_training_entity
    {
        public float month { get; set; }
        public float year { get; set; }
        public float previous_month_operational_cost { get; set; }
        public float last_three_months_cost { get; set; }
        public float last_six_months_cost { get; set; }

        //TARGET VALUE
        public float total_month_operational_cost { get; set; }
    }
}
