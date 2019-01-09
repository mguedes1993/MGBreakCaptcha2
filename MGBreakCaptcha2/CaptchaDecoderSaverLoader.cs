using System;
using System.IO;
using Accord.IO;

namespace MGBreakCaptcha2
{
    public static class CaptchaDecoderSaverLoader
    {
        public static FileInfo SaveDecoder(this CaptchaDecoder captchaDecoder, DirectoryInfo directory)
        {
            string fileName = $"{captchaDecoder.GetType().Name}_{DateTime.Now:yyyyMMddHHmmss}_{captchaDecoder.Id}.gz";

            Log.Write(typeof(CaptchaDecoderSaverLoader), $"Saving CaptchaDecoder on '{fileName}'");

            string filePath = Path.Combine(directory.FullName, fileName);
            captchaDecoder.Save(filePath, SerializerCompression.GZip);
            return new FileInfo(filePath);
        }

        public static FileInfo SaveDecoder(this CaptchaDecoder captchaDecoder, FileInfo file)
        {
            string fileName = file.FullName;
            if (!fileName.EndsWith(".gz"))
                fileName += ".gz";

            Log.Write(typeof(CaptchaDecoderSaverLoader), $"Saving CaptchaDecoder on '{fileName}'");

            captchaDecoder.Save(fileName, SerializerCompression.GZip);
            return new FileInfo(fileName);
        }

        public static CaptchaDecoder LoadDecoder(FileInfo file)
        {
            Log.Write(typeof(CaptchaDecoderSaverLoader), $"Loading CaptchaDecoder from '{file.FullName}'");

            if (!file.Exists)
            {
                Log.Write(typeof(CaptchaDecoderSaverLoader), $"File not found");
                return null;
            }

            return Serializer.Load<CaptchaDecoder>(file.FullName, SerializerCompression.GZip);
        }
    }
}