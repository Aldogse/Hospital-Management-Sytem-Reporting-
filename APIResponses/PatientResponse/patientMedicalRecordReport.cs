using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PatientResponse
{
    public class patientMedicalRecordReport
    {
        public int patientId { get; set; }
        public string fullName { get; set; }
        public string address { get; set; }
        public string gender { get; set; }
        public List<prevMedicalRecordsResponse> prevMedRecs { get; set; }
    }
}
