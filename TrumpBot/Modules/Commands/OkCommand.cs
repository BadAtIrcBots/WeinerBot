using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrumpBot.Modules.Commands
{
    [Command.NoPrefix]
    public class OkCommand : ICommand
    {
        public string CommandName { get; } = "👌 Command";
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex("👌", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(string message, string channel, string nick, GroupCollection arguments = null, bool useCache = true)
        {
            int count = new Regex(Regex.Escape("👌")).Matches(message).Count;
            return new List<string>{ String.Concat(Enumerable.Repeat("👌", count))};
        }
    }
}
