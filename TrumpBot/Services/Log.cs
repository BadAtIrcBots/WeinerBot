using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
