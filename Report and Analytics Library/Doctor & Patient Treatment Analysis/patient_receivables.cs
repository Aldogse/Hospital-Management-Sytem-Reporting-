using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;
using Report_and_Analytics_Library.JournalEntry;

namespace Report_and_Analytics_Library.Doctor___Patient_Treatment_Analysis
{
    public class patient_receivables
    {
        [Key]
        public int receivable_id { get; set; }

        [ForeignKey("journal_Entry_Line")]
        public int journal_entry_line_id { get; set; }
        [JsonIgnore]
        public journal_entry_line journal_Entry_Line { get; set; }
        

        [ForeignKey("patientinfo")]
        public int patient_id { get; set; }
        [JsonIgnore]
        public patientinfo patientinfo { get; set; }


        public string invoice_number { get; set; }
        public decimal amount_due { get; set; }
        public DateOnly? due_date { get; set; }
        public patient_receivables_status status { get; set; }
    }
}
