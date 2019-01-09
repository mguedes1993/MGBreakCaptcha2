using System;

namespace MGBreakCaptcha2
{
    public static class Log
    {
        public static void Write(Type classType, string message)
        {
            Console.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss.fffff} - {classType.Name}: {message}");
        }
    }
}