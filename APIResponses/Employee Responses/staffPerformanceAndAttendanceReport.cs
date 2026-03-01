using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public  class staffPerformanceAndAttendanceReport
    {
        public string department { get; set; }
        public decimal departmentEvaluationTotalScore { get; set; }
        public decimal? departmentEvaluationAverageScore { get; set; }
        public int deparmentTotalPresentEmployee { get; set; }
        public int deparmentTotalLateEmployee { get; set; }
        public int deparmentTotalUndertimeEmployee  { get; set; }
        public int departmentTotalOffDuty { get; set; }
        public int departmentTotalOvertime { get; set; }
    }
}
