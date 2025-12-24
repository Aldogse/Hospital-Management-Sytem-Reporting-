using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.PropertyAndManagementResponse
{
    public class yearBedsDistributionSummaryResponse
    {
        public int year { get; set; }
        public double occupied_beds { get; set; }
        public double available_beds { get; set; }
        public double broken_beds { get; set; }
        public int total_beds { get; set; }
        public List<month_admission_and_discharge_report> monthsAdmissionReport { get; set; }
    }
}

