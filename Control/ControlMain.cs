using MGBreakCaptcha2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MGBreakCaptcha2.ColorML;
using MGBreakCaptcha2.MLEngine;
using static System.Console;

namespace Control
{
    class ControlMain
    {
        static void Main(string[] args)
        {
            string[] paths =
            {
                @".\Datasets\TJ_ESAJ_COLOR",
                @".\Datasets\TJ_PJE",
                @".\Datasets\TRT_PJE"
            };

            string[] courts =
            {
                @"TJ_ESAJ_COLOR",
                @"TJ_PJE",
                @"TRT_PJE"
            };

            CaptchaDecoder captchaDecoder = new CaptchaDecoder();
            captchaDecoder.LoadDataSet(paths, courts);
            captchaDecoder.Learn();
            captchaDecoder.SaveDecoder(new FileInfo(@".\CaptchaDecoder.gz"));

            CaptchaDecoder newCaptchaDecoder =
                CaptchaDecoderSaverLoader.LoadDecoder(new FileInfo(@".\CaptchaDecoder.gz"));

            WriteLine();
            WriteLine("CAPTCHAs predicted wrong...");
            for (int i = 0; i < paths.Length; i++)
            {
                DirectoryInfo directory = new DirectoryInfo(paths[i]);
                string[] fileTypes = { "*.png", "*.jpeg", "*.jpg", "*.jfif" };
                FileInfo[] files =
                    fileTypes
                        .SelectMany(t => directory.GetFiles(t, SearchOption.TopDirectoryOnly))
                        .OrderBy(f => f.Name)
                        .ToArray();

                foreach (FileInfo file in files)
                {
                    string result = newCaptchaDecoder.Predict(file.FullName, courts[i]);

                    if (!file.Name.Split('-')[0].Equals(result))
                    {
                        WriteLine($"Court: {courts[i]} | Correct Label: {file.Name.Split('-')[0]} | Predicted Label: {result}");
                    }
                }
            }

            ReadKey();
        }
    }
}
