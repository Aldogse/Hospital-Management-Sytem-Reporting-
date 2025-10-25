using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.Historical_report.Models
{
    public class month_pharmacy_sales
    {
        [Key]
        public int reportId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public int totalTransactions { get; set; }
        public decimal? totalSales { get; set; }
        public string? topSellingItem { get; set; }
    }
}
