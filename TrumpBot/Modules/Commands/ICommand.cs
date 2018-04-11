using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TrumpBot.Modules.Commands
{
    internal interface ICommand
    {
        string CommandName { get; }
        List<Regex> Patterns { get; set; }
        
        List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true);
    }
}
