using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Backtrace;
using WeinerBot.Configs;
using WeinerBot.Models.Config;

namespace WeinerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            IrcConfigModel.IrcSettings settings =
                ConfigHelpers.LoadConfig<IrcConfigModel.IrcSettings>(ConfigHelpers.ConfigPaths.IrcConfig);

            if (settings.AutoRestart)
            {
               AppDomain.CurrentDomain.UnhandledException += Restart; 
            }

            new IrcBot(settings);
        }

        private static void Restart(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exception = (Exception) args.ExceptionObject;

            StreamWriter exceptionLogStreamWriter = File.AppendText("exceptions.log");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: Exception occurred in {exception.Source}");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: {exception.Message}");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: {exception.StackTrace}");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: Process file name: {Process.GetCurrentProcess().MainModule?.FileName}");
            exceptionLogStreamWriter.Close();
            Services.Backtrace.GetBacktraceClient()?.Send(exception);
            Thread.Sleep(1000); // Give us time to stop it if this is just happening in a loop!
            var process = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true
            };
            Process.Start(process);
            Environment.Exit(1);
        }
    }
}
