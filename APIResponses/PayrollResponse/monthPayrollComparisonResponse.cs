using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIResponses.PayrollResponse
{
    public class monthPayrollComparisonResponse
    {
        public int baseMonth { get; set; }
        public int baseYear { get; set; }
        public int? BaseTotalEmployees { get; set; }
        public decimal? BaseTotalGrossPay { get; set; }
        public decimal? BaseTotalDeductions { get; set; }
        public decimal? BaseTotalNetPay { get; set; }

        public int comparedMonth { get; set; }
        public int comparedYear { get; set; }
        public int? comparedTotalEmployees { get; set; }
        public decimal? comparedTotalGrossPay { get; set; }
        public decimal? comparedTotalDeductions { get; set; }
        public decimal? comparedTotalNetPay { get; set; }
    }
}
