using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.PropertyAndManagementResponse
{
    public class monthBedsAndDischargesQueryResponse
    {
        public int total_beds { get; set; }
        public int occupied_beds { get; set; }
        public int available_beds { get; set; }
        public int recently_discharged { get; set; }
        public int broken_beds { get; set; }
        public List<month_admission_and_discharge_report> months { get; set; }
    }
}
