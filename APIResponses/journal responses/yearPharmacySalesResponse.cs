using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIResponses.Historical_report.Models;

namespace APIResponses.journal_responses
{
    public class yearPharmacySalesResponse
    {
        public int year { get; set; }
        public int totalTransactions { get; set; }
        public decimal? totalSales { get; set; }
        public string? topSellingItem { get; set; }
        public List<month_pharmacy_sales>monthSales { get; set; }
    }
}
