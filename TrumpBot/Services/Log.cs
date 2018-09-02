using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TrumpBot.Services
{
    internal static class Log
    {
        internal static void LogToFile(string message, string fileName, [CallerMemberName]string callerMemberName = "")
        {
            if (string.IsNullOrEmpty(callerMemberName))
            {
                callerMemberName = "Unknown caller";
            }
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            StreamWriter logStreamWriter = File.AppendText($"Logs\\{fileName}");
            logStreamWriter.WriteLine($"[{DateTime.Now}] ({callerMemberName}): {message}");
            logStreamWriter.Close();
        }
    }
}
