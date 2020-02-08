using System;
using System.Collections.Generic;

namespace iMask.EF.Models
{
    public class Query
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public virtual List<QueryAmount> QueryAmounts { get; set; }
    }
}