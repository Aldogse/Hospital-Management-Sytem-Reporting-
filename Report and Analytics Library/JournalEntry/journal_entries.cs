using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class journal_entries
    {
        [Key]
        public int entry_id { get; set; }
        public DateOnly entry_date { get; set; }
        public string description { get; set; }
        public string? reference { get; set; }
        public string status { get; set; }
        public string created_by { get; set; }
    }
}
