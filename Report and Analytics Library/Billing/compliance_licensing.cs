using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Crmf;
using Report_and_Analytics_Library.HR;

namespace Report_and_Analytics_Library.Billing
{
    public class compliance_licensing
    {
        [Key]
        public int compliance_id { get; set; }

        [ForeignKey("employees")]
        public int employee_id { get; set; }
        public hr_employees employees { get; set; }

        public string credential_name { get; set; }
        public string license_no { get; set; }
        public string issued_by { get; set; }
        public DateOnly issued_date { get; set; }
        public DateOnly expiry_date { get; set; }
        public int status { get; set; }
        public string? document_file { get; set; }
        public DateTime last_verified_date { get; set; }
    }
}
