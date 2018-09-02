using SharpRaven;
using TrumpBot.Configs;
using TrumpBot.Models.Config;

namespace TrumpBot.Services
{
    internal static class Raven
    {
        internal static RavenClient GetRavenClient()
        {
            IrcConfigModel.IrcSettings settings =
                ConfigHelpers.LoadConfig<IrcConfigModel.IrcSettings>(ConfigHelpers.ConfigPaths.IrcConfig);
            return settings.RavenDsn == null ? null : new RavenClient(settings.RavenDsn);
        }
    }
}
