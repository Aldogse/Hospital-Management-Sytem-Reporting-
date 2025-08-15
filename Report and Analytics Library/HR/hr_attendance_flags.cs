using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.HR
{
    public class hr_attendance_flags
    {
        [Key]
        public int flag_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }

        public DateOnly attendance_date { get; set; }
        public attendance_flags flag_type { get; set; }
        public string? remarks { get; set; }
        public DateTime created_at { get; set; }
    }
}
