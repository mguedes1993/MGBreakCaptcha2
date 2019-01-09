using System;
using System.Collections.Generic;
using System.IO;
using MGBreakCaptcha2.ImageEngine;

namespace MGBreakCaptcha2
{
    [Serializable]
    class DataSet
    {
        public string CourtName { get; set; }
        public DirectoryInfo CourtDirectory { get; set; }
        public IImageEngine CourtImageEngine { get; set; }
        public List<DataSetItem> Items { get; set; }
    }
}