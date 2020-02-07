using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace iMask.Data.Models
{
    public class CSV
    {
        [Index(0)]
        public string 醫事機構代碼 { get; set; }
        [Index(1)]
        public string 醫事機構名稱 { get; set; }
        [Index(2)]
        public string 醫事機構地址 { get; set; }
        [Index(3)]
        public string 醫事機構電話 { get; set; }
        [Index(4)]
        public string 成人口罩總剩餘數 { get; set; }
        [Index(5)]
        public string 兒童口罩剩餘數 { get; set; }
        [Index(6)]
        public string 來源資料時間 { get; set; }
    }
}
