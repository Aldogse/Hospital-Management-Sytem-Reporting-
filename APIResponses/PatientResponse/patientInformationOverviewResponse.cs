using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PatientResponse
{
    public class patientInformationOverviewResponse
    {
        public double averageAge {  get; set; }
        public int maleCount { get; set; }
        public int femaleCount { get; set; }
        public List<patientInformationResponse> patients { get; set; }
        public List<object>ages { get; set; }
    }
}
