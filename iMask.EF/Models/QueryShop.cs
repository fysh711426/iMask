using System;

namespace iMask.EF.Models
{
    public class QueryShop
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public int Rank { get; set; }
        public int ShopId { get; set; }

        public virtual Query Query { get; set; }
        public virtual Shop Shop { get; set; }
}