using System;

namespace iMask.EF.Models
{
    public class QueryAmount
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public int Rank { get; set; }
        public int AmountId { get; set; }

        public virtual Query Query { get; set; }
        public virtual Amount Amount { get; set; }
    }
}