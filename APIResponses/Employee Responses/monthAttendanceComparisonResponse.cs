using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public  class monthAttendanceComparisonResponse
    {
        //BASE YEAR
        public int baseMonth { get; set; }
        public int baseYear{ get; set; }
        public int? basePresent { get; set; }
        public int? baseLate { get; set; }
        public int? baseUnderTime { get; set; }

        //COMPARED MONTH AND YEAR
        public int partnerMonth { get; set; }
        public int partnerYear { get; set; }
        public int? partnerPresent { get; set; }
        public int? partnerLate { get; set; }
        public int? partnerUnderTime { get; set; }

    }
}
