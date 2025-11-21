using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public class monthAttendanceReportResponse
    {
        public int? present { get; set; }
        public int? absent { get; set; }
        public int? late { get; set; }
        public int? leave { get; set; }
        public int? underTime { get; set; }
    }
}
