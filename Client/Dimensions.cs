using System;

namespace Treachery.Client
{
    public class Dimensions
    {
        public float Ratio { get; set; }
        public int X { get; set; }
        //public int Y { get; set; }
        public float DivWidth { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }

        //public float CanvasScrollX { get; set; }
        public float ScrollY { get; set; }

        public int TranslateClientXToRelativeX(double clientX)
        {
            return Convert.ToInt32((clientX - X) / ScaleX);
        }

        public int TranslateClientYToRelativeY(double clientY)
        {
            return Convert.ToInt32((clientY - (int)ScrollY) / ScaleY);
        }
    }
}
