using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class journal_entry_lines
    {
        [Key]
        public int line_id { get; set; }
        public int entry_id { get; set; }
        public string account_name { get; set; }
        public decimal credit { get; set; }
        public decimal debit { get; set; }
        public string memo { get; set; }
    }
}
