using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace APIResponses.Employee_Responses
{
    public class doctorDetailsAndEvaluationSummaryResponse
    {
        public string department { get; set; }
        public string specialization { get; set; }
        public string role { get; set; }
        public string employmentType { get; set; }
        public string educationalStatus { get; set; }
        public string degreeType { get; set; }
        public string medicalSchool { get; set; }
        public int graduationYear { get; set; }
        public string licenseType { get; set; }
        public string licenseNumber { get; set; }
        public DateOnly? licenseIssued { get; set; }
        public DateOnly? licenseExpiry { get; set; }
        public List<doctorSummaryEvaluationResponse> evaluation_records { get; set; }
    }
}
