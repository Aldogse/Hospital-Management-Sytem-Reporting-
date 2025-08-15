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
    public class hr_applicant_documents
    {
        [Key]
        public int document_id { get; set; }


        [ForeignKey("Hr_Application")]
        public int? applicant_id { get; set; }
        [JsonIgnore]
        public hr_application Hr_Application { get; set; }

        public string? document_type { get; set; }
        public string? file_path { get; set; }
        public DateTime uploaded_at { get; set; }
    }
}
