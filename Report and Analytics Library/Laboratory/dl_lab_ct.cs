using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Laboratory
{
    public class dl_lab_ct
    {
        [Key]
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("scheduleID")]
        public int scheduleID { get; set; }

        [JsonPropertyName("patientID")]
        public int patientID { get; set; }

        [JsonPropertyName("testType")]
        public string? testType { get; set; }

        [JsonPropertyName("findings")]
        public string? findings { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime created_at { get; set; }

        [JsonPropertyName("processed_by")]
        public int? processed_by { get; set; }
    }
}
