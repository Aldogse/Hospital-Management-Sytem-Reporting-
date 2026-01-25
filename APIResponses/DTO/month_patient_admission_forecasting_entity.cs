using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_patient_admission_forecasting_entity
    {
        public float month { get; set; }
        public float year { get; set; }

        //TARGET VALUE
        public float total_admission { get; set; }

        //INPUT FEATURES
        public float prev_month_admission { get; set; }
        public float last_three_month_admission { get; set; }
        public float last_sixth_month_admission { get; set; }
    }
}
