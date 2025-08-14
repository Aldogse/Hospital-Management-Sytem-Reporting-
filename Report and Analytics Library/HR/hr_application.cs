using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_application
    {
        [Key]
        public int applicant_id { get; set; }
        public string? first_name { get; set; }
        public string?  last_name { get; set; }
        public string? gender { get; set; }
        public DateOnly? date_of_birth { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? address { get; set; }
        public string? profession { get; set; }
        public string? role { get; set; }
        public string? specialization { get; set; }
        public string? status { get; set; }
        public DateTime date_applied { get; set; }
        public string? document_type { get; set; }
        public string? file_path { get; set; }
        public DateTime uploaded_at { get; set; }
    }
}
