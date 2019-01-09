using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Accord.Imaging.Converters;
using Accord.Math;
using MGBreakCaptcha2.ColorML;

namespace MGBreakCaptcha2.ImageEngine
{
    [Serializable]
    public class TJ_ESAJ_COLOR_ImageEngine : IImageEngine
    {
        private readonly int[] _delimiters = {24, 51, 78, 105, 132, 159, 186, 213};

        public Bitmap[] ExtractCharacters(Bitmap imageBitmap, ColorsEnum? color = null)
        {
            ImageToMatrix imageToMatrix = new ImageToMatrix();
            imageBitmap = new Bitmap(imageBitmap, new Size(260, 51));

            List<Bitmap> charactersBitmap = new List<Bitmap>();

            foreach (int delimiter in _delimiters)
            {
                Bitmap character = imageBitmap
                    .Clone(new Rectangle {X = delimiter, Y = 8, Width = 27, Height = 29}, imageBitmap.PixelFormat);

                charactersBitmap.Add(character);
            }

            if (color != null)
            {
                List<Bitmap> charactersBitmapColor = new List<Bitmap>();
                foreach (Bitmap character in charactersBitmap)
                {
                    imageToMatrix.Convert(character, out Color[][] imageColors);
                    Color characterColor = imageColors.SelectMany(c => c).GroupBy(c => c.Name).OrderByDescending(g => g.Count()).ElementAt(1).ElementAt(0);
                    
                    ColorsEnum result = ColorDecoder.Decode(characterColor);
                    if (result.Equals(color))
                    {
                        charactersBitmapColor.Add(character);
                    }
                }

                charactersBitmap = charactersBitmapColor;
            }

            return charactersBitmap.ToArray();
        }

        public double[] ConvertImageToDouble(Bitmap imageBitmap)
        {
            ImageToMatrix imageToMatrix = new ImageToMatrix();

            imageToMatrix.Convert(imageBitmap, out byte[][] imageBytes);
            imageToMatrix.Convert(imageBitmap, out Color[][] imageColors);

            Color characterColor = imageColors.SelectMany(c => c).GroupBy(c => c.Name).OrderByDescending(g => g.Count()).ElementAt(1).ElementAt(0);
            
            for (var i = 0; i < imageBytes.Length; i++)
            {
                for (var j = 0; j < imageBytes[i].Length; j++)
                {
                    if (imageColors[i][j].Name == characterColor.Name)
                    {
                        imageBytes[i][j] = 1;
                    }
                    else
                    {
                        imageBytes[i][j] = 0;
                    }
                }
            }

            return imageBytes.Flatten().ToDouble();
        }
    }
}