using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.HR
{
    public class hr_leave
    {
        [Key]
        public int leave_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        public hr_employees hr_Employees { get; set; }

        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? profession { get; set; }
        public string? role { get; set; }
        public string? department { get; set; }
        public DateOnly? leave_start_date { get; set; }
        public DateOnly? leave_end_date { get; set; }
        public string? leave_type { get; set; }
        public string? leave_status { get; set; }
        public DateOnly? approval_date { get; set; }
        public string? leave_reason { get; set; }
        public string? medical_cert { get; set; }
        public DateTime submit_at { get; set; }
    }
}
