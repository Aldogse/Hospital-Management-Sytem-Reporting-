using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Billing
{
    public class claim_notes
    {
        [Key]
        public int claim_note_id { get; set; }

        public int insurance_claims_id { get; set; }

        public string note { get; set; }
        public string added_by { get; set; }
    }
}
