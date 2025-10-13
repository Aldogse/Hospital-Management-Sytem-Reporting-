using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Employee_Responses
{
    public class staffInformationReportResponse
    {
        public int totalEmployees { get; set; }
        public int activeStaff { get; set; }
        public List<staffBasicInformation> employees { get; set; }
    }
}
