using System;
using System.Threading;
using log4net;
using Meebey.SmartIrc4net;
using SharpRaven;
using TrumpBot.Configs;
using TrumpBot.Models;
using CoinMarketCapApi.Models;
using SharpRaven.Data;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules
{
    internal class TetherMonitor
    {
        private IrcClient _ircClient;
        private TetherConfigModel _config;
        private Thread _thread;
        private ILog _log = LogManager.GetLogger(typeof(TetherMonitor));
        private RavenClient _ravenClient = Services.Raven.GetRavenClient();

        internal TetherMonitor(IrcClient ircClient)
        {
            _ircClient = ircClient;
            LoadConfig();
            if (!_config.Enabled)
            {
                _log.Debug("TetherMonitor has been disabled in the config. Exiting module");
                return;
            }
            Thread newThread = new Thread(() => CheckTether());
            _thread = newThread;
            _thread.Start();
            _log.Debug("TetherMonitor created and started");

        }

        internal void LoadConfig()
        {
            _config = ConfigHelpers.LoadConfig<TetherConfigModel>(ConfigHelpers.ConfigPaths.TetherConfig);
            _log.Debug("TetherMonitor has loaded its config");
        }

        internal void SaveConfig()
        {
            ConfigHelpers.SaveConfig(_config, ConfigHelpers.ConfigPaths.TetherConfig);
        }
        
        private void CheckTether()
        {
            while (true)
            {
                Thread.Sleep(_config.CheckInterval * 1000 * 60);
                _log.Debug("Checking CoinMarketCap tether ticker");
                TickerModel ticker;
                try
                {
                    ticker = CoinMarketCapApi.Api.TickerApi.GetTicker("tether").Result[0];
                }
                catch (Exception e) // Thank you CoinMarketCap for breaking your shitty API and forcing me to do this cancerous shit
                {
                    Raven.GetRavenClient()?.Capture(new SentryEvent(e));
                    if (e.InnerException != null)
                    {
                        Raven.GetRavenClient()?.Capture(new SentryEvent(e.InnerException));
                    }
                    continue;
                }
                decimal supply = Convert.ToDecimal(ticker.AvailableSupply);
                var colours = new Colours();
                if (_config.LastValue != supply)
                {
                    _log.Debug($"Available supply has increased to {ticker.AvailableSupply}");
                    foreach (string channel in _config.Channels)
                    {
                        if (_ircClient.IsJoined(channel))
                        {
                            if (_config.LastValue < supply)
                            {
                                _ircClient.SendMessage(SendType.Message, channel,
                                    $"Tether available supply has increased! There are now {supply:n0} tethers available" +
                                    $" ({IrcConstants.IrcColor}{colours.Green}+{supply - _config.LastValue:n0}{IrcConstants.IrcNormal})");
                                continue;
                            }
                            _ircClient.SendMessage(SendType.Message, channel, $"Tether available supply has reduced! There are now {supply:n0} tethers available" +
                                                                              $" ({IrcConstants.IrcColor}{colours.LightRed}{supply - _config.LastValue:n0}{IrcConstants.IrcNormal})");

                        }
                    }
                    _config.LastValue = supply;
                    _config.LastChange = DateTime.UtcNow;
                    SaveConfig();
                }
            }
        }
    }
}