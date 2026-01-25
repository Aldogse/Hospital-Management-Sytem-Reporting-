using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.forecast_results
{
    public class month_patient_admission_forecast_result
    {
        [Key]
        public int training_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        //TARGET VALUE
        public int total_admission { get; set; }
    }
}
