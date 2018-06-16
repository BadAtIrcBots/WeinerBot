using System;
using System.IO;
using System.Threading;
using SharpRaven;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot
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

            IrcBot ircBot = new IrcBot(settings);
        }

        private static void Restart(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exception = (Exception) args.ExceptionObject;

            StreamWriter exceptionLogStreamWriter = File.AppendText("exceptions.log");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: Exception occurred in {exception.Source}");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: {exception.Message}");
            exceptionLogStreamWriter.WriteLine($"[{DateTime.Now}]: {exception.StackTrace}");
            exceptionLogStreamWriter.Close();
            RavenClient ravenClient = Services.Raven.GetRavenClient();
            ravenClient?.Capture(new SentryEvent(exception));
            Thread.Sleep(1000); // Give us time to stop it if this is just happening in a loop!
            System.Diagnostics.Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location,
                string.Join(" ", Environment.GetCommandLineArgs()));
            Environment.Exit(1);
        }
    }
}
