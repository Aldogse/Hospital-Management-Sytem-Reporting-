using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Laboratory
{
    public class dl_results
    {
        [Key]
        public int resultID { get; set; }
        public int scheduleID { get; set; }
        public int patientID { get; set; }
        public DateTime resultDate { get; set; }
        public string? status { get; set; }
        public string? result { get; set; }
        public string? remarks { get; set; }
        public string? received_by { get; set; }
        public int validated_by { get; set; }
    }
}
