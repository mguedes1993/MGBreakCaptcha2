using System;
using System.Drawing;

namespace MGBreakCaptcha2
{
    [Serializable]
    class DataSetItem
    {
        public Bitmap originalImage { get; set; }
        public string originalLabel { get; set; }
        public Bitmap[] charactersBitmap { get; set; }
        public double[][] charactersDouble { get; set; }
        public int[] charactersLabelsInt { get; set; }
        public char[] charactersLabelsChar { get; set; }
    }
}