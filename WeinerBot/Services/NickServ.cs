using System;
using Meebey.SmartIrc4net;
using WeinerBot.Models.Config;

namespace WeinerBot.Services
{
    public class NickServ
    {
        private IrcClient _client;
        private IrcConfigModel.IrcSettings _settings;

        public NickServ(IrcClient client, IrcConfigModel.IrcSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        public void Identify()
        {
            if (_settings.NickservPassword == null)
            {
                throw new NoNickServPasswordException("NickServ password is null");
            }
            _client.SendMessage(SendType.Message, "NickServ", $"IDENTIFY {_settings.NickservPassword}");
        }

        public void Logout()
        {
            _client.SendMessage(SendType.Message, "NickServ", "LOGOUT");
        }

        public class NoNickServPasswordException : Exception
        {
            public NoNickServPasswordException(string message) : base(message) { }
        }
    }
}
