using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace APIResponses.Employee_Responses
{
    public class monthShiftAndDutyReportResponse
    {
        public int? totalAppointments { get; set; }
        public int? doctorDuties { get; set; }
        public int? nurseDuties { get; set; }
        public int? completed { get; set; }
        public int? cancelled  { get; set; }
        public List<duty_assignment> appointments { get; set; }
    }
}
