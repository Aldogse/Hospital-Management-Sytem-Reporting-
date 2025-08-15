using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Billing;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class license_expiry
    {
        [Key]
        public int alert_id { get; set; }

        [ForeignKey("hr_Employees")]
        public int employee_id { get; set; }
        [JsonIgnore]
        public hr_employees hr_Employees { get; set; }


        [ForeignKey("compliance_Licensing")]
        public int compliance_id { get; set; }
        [JsonIgnore]
        public compliance_licensing compliance_Licensing { get; set; }

        public int alert_type { get; set; }
        public DateOnly alert_date { get; set; }
        public int sent_status { get; set; }
        public int notification_channel { get; set; }
    }
}
