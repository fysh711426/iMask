using System;
using System.Collections.Generic;

namespace iMask.EF.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public virtual ICollection<Amount> Amounts { get; set; }
        public virtual ICollection<QueryShop> QueryShops { get; set; }
    }
}