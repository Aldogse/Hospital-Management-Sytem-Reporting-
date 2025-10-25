using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PatientResponse
{
    public class patientInformationResponse
    {
        public int patientId { get; set; }
        public string fullName { get; set; }
        public string gender { get; set; }
        public int age { get; set; }
        public string contact { get; set; }
    }
}
