using Report_and_Analytics_Library.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_and_Analytics_Library.Property_Management
{
    public class beds
    {
        [Key]
        public int bed_id { get; set; }
        public string bed_number { get; set; }
        public string ward { get; set; }
        public string? room_number { get; set; }
        public bedType bed_type { get; set; }
        public bedStatus status { get; set; }
        public string? notes { get; set; }
        public List<bed_assignments> bed_Assignments { get; set; }
    }
}
