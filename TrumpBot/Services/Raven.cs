using SharpRaven;
using TrumpBot.Configs;
using TrumpBot.Models;

namespace TrumpBot.Services
{
    internal static class Raven
    {
        internal static RavenClient GetRavenClient()
        {
            IrcConfigModel.IrcSettings settings = (IrcConfigModel.IrcSettings)new IrcConfig().LoadConfig();
            return settings.RavenDsn == null ? null : new RavenClient(settings.RavenDsn);
        }
    }
}
