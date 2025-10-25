using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PatientResponse
{
    public class prevMedicalRecordsResponse
    {
        public int recordId {  get; set; }
        public string conditionName { get; set; }
        public DateTime? diagnosisDate { get; set; }
        public string? notes { get; set; }
    }
}
