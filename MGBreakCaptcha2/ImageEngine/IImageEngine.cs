using System.Drawing;
using MGBreakCaptcha2.ColorML;

namespace MGBreakCaptcha2.ImageEngine
{
    public interface IImageEngine
    {
        Bitmap[] ExtractCharacters(Bitmap imageBitmap, ColorsEnum? color = null);

        double[] ConvertImageToDouble(Bitmap imageBitmap);
    }
}