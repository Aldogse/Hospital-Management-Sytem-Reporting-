using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Report_and_Analytics_Library.Enums;

namespace Report_and_Analytics_Library.JournalEntry
{
    public class journal_account
    {
        [Key]
        public int account_id { get; set; }
        public string account_name { get; set; }
        public journal_account_type account_type { get; set; }
        public DateTime created_at { get; set; }
    }
}
