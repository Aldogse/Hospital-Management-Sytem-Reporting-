using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class journal_entry_line
    {
        [Key]
        public int journal_entry_line_id { get; set; }


        [ForeignKey("journal_Entry")]
        public int journal_entry_id { get; set; }
        [JsonIgnore]
        public journal_entry journal_Entry { get; set; }


        [ForeignKey("journal_Account")]
        public int account_id { get; set; }
        [JsonIgnore]
        public journal_account journal_Account { get; set; }

        public decimal debit_amount { get; set; }
        public decimal credit_amount { get; set; }
    }
}
