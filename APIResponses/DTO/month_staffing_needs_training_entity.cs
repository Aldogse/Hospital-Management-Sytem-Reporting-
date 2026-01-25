using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.DTO
{
    public class month_staffing_needs_training_entity
    {
        public int month { get; set; }
        public int year { get; set; }
        public string department { get; set; }   // Keep as string

        public float avg_staff_present { get; set; }
        public float avg_working_hours { get; set; }
        public float avg_overtime_hours { get; set; }

        public float total_working_hours_needed { get; set; } // label or feature
        public float total_staff_needed { get; set; }         // label
    }
}
