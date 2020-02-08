using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FixFlex
{
    public class BoxComponent : Line.Messaging.BoxComponent
    {
        public string BackgroundColor { get; set; }
        public string CornerRadius { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string OffsetTop { get; set; }
        public string offsetBottom { get; set; }
        public string OffsetStart { get; set; }
        public string OffsetEnd { get; set; }
        public string PaddingAll { get; set; }
        public string PaddingStart { get; set; }
        public string PaddingEnd { get; set; }
        public string PaddingTop { get; set; }
        public string PaddingBottom { get; set; }
        public string Position { get; set; }
    }

    public class TextComponent : Line.Messaging.TextComponent
    {
        public string OffsetTop { get; set; }
    }

    public class FillerComponent : Line.Messaging.FillerComponent
    {
        public int Flex { get; set; }
    }

    public class BubbleContainer : Line.Messaging.BubbleContainer
    {
        public string Size { get; set; }
    }
}
