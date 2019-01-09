using System;
using System.Drawing;
using System.IO;
using Accord.IO;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;

namespace MGBreakCaptcha2.ColorML
{
    public static class ColorDecoder
    {
        private static readonly FileInfo ColorFileInfo = new FileInfo(@".\Color.dat");

        public static ColorsEnum Decode(Color color)
        {
            MulticlassSupportVectorMachine<Linear> engine;

            if (File.Exists(ColorFileInfo.FullName))
            {
                engine = Serializer.Load<MulticlassSupportVectorMachine<Linear>>(ColorFileInfo.FullName);
            }
            else
            {
                MulticlassSupportVectorLearning<Linear> learning = new MulticlassSupportVectorLearning<Linear>();
                engine = learning.Learn(ColorData.Data, ColorData.Label);
                engine.Save(ColorFileInfo.FullName);
            }
            
            int resultInt = engine.Decide(new double[] {color.R, color.G, color.B});

            Enum.TryParse(resultInt.ToString(), out ColorsEnum resultColorsEnum);

            return resultColorsEnum;
        }
    }
}