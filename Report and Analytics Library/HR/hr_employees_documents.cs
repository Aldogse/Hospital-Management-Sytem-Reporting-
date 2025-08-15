using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_employees_documents
    {
        [Key]
        public int document_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int? employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }

        public string? document_type { get; set; }
        public string? file_path { get; set; }
        public DateTime uploaded_at { get; set; }
    }
}
