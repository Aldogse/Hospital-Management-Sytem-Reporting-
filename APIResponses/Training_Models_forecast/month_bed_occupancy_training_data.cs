using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Training_Models
{
    public  class month_bed_occupancy_training_data
    {
        [Key]
        public int report_id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int total_beds { get; set; }

        //target values
        public int occupied_beds { get; set; }

        public int recently_discharged { get; set; }
        public float bed_occupancy_rate { get; set; }
        public float broken_bed_rate { get; set; }
    }
}
