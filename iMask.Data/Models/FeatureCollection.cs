using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace iMask.Data.Models
{
    public class FeatureCollection
    {
        public string type { get; set; }
        public List<Feature> features { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }

        public Data properties { get; set; }
        public Point geometry { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int mask_adult { get; set; }
        public int mask_child { get; set; }
        public string updated { get; set; }
        public string available { get; set; }
        public string note { get; set; }
        public string custom_note { get; set; }
        public string website { get; set; }
    }

    public class Point
    {
        public string type { get; set; }
        public List<decimal> coordinates { get; set; }
    }
}