using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Newtonsoft.Json;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.AdminCommands
{
    [Admin.RequiredRight(AdminConfigModel.Right.Admin)]
    internal class GeezAdminCommand : IAdminCommand
    {
        public string Name { get; } = "GeezAdmin";
        public List<Regex> Patterns { get; } = new List<Regex>
        {
            new Regex(@"^geez (\d+) (\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot)
        {
            var min = Convert.ToInt32(values[1].Value);
            var max = Convert.ToInt32(values[2].Value);

            if (min < 1)
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Min value too small");
                return;
            }
            if (max > 2000)
            {
                client.SendMessage(SendType.Message, eventArgs.Data.Channel, "Max value is stupid");
                return;
            }

            List<int> randomValues = new List<int> {min,max};

            File.WriteAllText("Config\\geez.json", JsonConvert.SerializeObject(randomValues));
            client.SendMessage(SendType.Message, eventArgs.Data.Channel, $"Modified geez config with {string.Join(", ", randomValues.ToArray())}");
        }
    }
}