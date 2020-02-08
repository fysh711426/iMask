using System;
using System.Collections.Generic;

namespace iMask.EF.Models
{
    public class Amount
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DateTime? DateTime { get; set; }
        public int? AdultAmount { get; set; }
        public int? ChildAmount { get; set; }

        public virtual ICollection<QueryAmount> QueryAmounts { get; set; }
    }
}