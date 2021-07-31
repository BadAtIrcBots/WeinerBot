using Backtrace;
using Backtrace.Model;
using WeinerBot.Configs;
using WeinerBot.Models.Config;

namespace WeinerBot.Services
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