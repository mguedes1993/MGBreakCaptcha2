﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Accord.Imaging.Converters;
using Accord.Imaging.Filters;
using Accord.Math;
using MGBreakCaptcha2.ColorML;

namespace MGBreakCaptcha2.ImageEngine
{
    [Serializable]
    public class TRT_PJE_ImageEngine : IImageEngine
    {
        private readonly int[] _delimiters = {13, 29, 45, 61, 77, 93};

        public Bitmap[] ExtractCharacters(Bitmap imageBitmap, ColorsEnum? color = null)
        {
            imageBitmap = new Bitmap(imageBitmap, new Size(120, 35));

            List<Bitmap> charactersBitmap = new List<Bitmap>();

            foreach (int delimiter in _delimiters)
            {
                Bitmap character = imageBitmap
                    .Clone(new Rectangle {X = delimiter, Y = 10, Width = 16, Height = 23}, imageBitmap.PixelFormat);

                charactersBitmap.Add(character);
            }

            return charactersBitmap.ToArray();
        }

        public double[] ConvertImageToDouble(Bitmap imageBitmap)
        {
            Grayscale gs = new Grayscale(0, 0, 0);
            ImageToMatrix imageToMatrix = new ImageToMatrix();

            imageBitmap = gs.Apply(imageBitmap);
            imageToMatrix.Convert(imageBitmap, out byte[][] imageBytes);

            foreach (byte[] t in imageBytes)
            {
                for (var j = 0; j < t.Length; j++)
                {
                    if (t[j] > 190)
                    {
                        t[j] = 1;
                    }
                    else
                    {
                        t[j] = 0;
                    }
                }
            }

            return imageBytes.Flatten().ToDouble();
        }
    }
}