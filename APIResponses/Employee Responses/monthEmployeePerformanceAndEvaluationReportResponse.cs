using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis;

namespace APIResponses.Employee_Responses
{
    public class monthEmployeePerformanceAndEvaluationReportResponse
    {
        public int? totalEmployeesEvaluated { get; set; }
        public decimal? monthAveragePerformanceScore  { get; set; }
        public decimal? monthNumberOfLowPerformers { get; set; }
        public List<evaluation_summary_reports> report { get; set; }
    }
}
