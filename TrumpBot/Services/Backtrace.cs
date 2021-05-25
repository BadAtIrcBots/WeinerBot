using Backtrace;
using Backtrace.Model;
using TrumpBot.Configs;
using TrumpBot.Models.Config;

namespace TrumpBot.Services
{
    public static class Backtrace
    {
        public static BacktraceClient GetBacktraceClient()
        {
            IrcConfigModel.IrcSettings settings =
                ConfigHelpers.LoadConfig<IrcConfigModel.IrcSettings>(ConfigHelpers.ConfigPaths.IrcConfig);
            if (settings.BacktraceSubmitUrl == null || settings.BacktraceToken == null)
            {
                return null;
            }
            var credentials = new BacktraceCredentials(settings.BacktraceSubmitUrl, settings.BacktraceToken);
            var client = new BacktraceClient(credentials);
            client.Attributes.Add("Network", settings.ConnectionUri.ToString());
            client.Attributes.Add("BotName", settings.Nick);
            return client;
        }
    }
}