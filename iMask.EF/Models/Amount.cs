using System;

namespace iMask.EF.Models
{
    public class Amount
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public DateTime DateTime { get; set; }
        public int AdultAmount { get; set; }
        public int ChildAmount { get; set; }
        public bool IsEnable { get; set; }

        public virtual Shop Shop { get; set; }
    }
}