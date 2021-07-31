using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;

namespace WeinerBot.Modules.AdminCommands
{
    public interface IAdminCommand
    {
        string Name { get; }
        List<Regex> Patterns { get; }

        void RunCommand(IrcClient client, GroupCollection values, IrcEventArgs eventArgs, IrcBot ircBot);
    }
}
