using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class journal_entry
    {
        [Key]
        public int journal_entry_id { get; set; }
        public DateOnly entry_date { get; set; }
        public string description { get; set; }
        public string? source_document { get; set; }
        public DateTime created_at { get; set; }
    }
}
