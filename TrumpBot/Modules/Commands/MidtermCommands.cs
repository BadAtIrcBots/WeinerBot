using System.Collections.Generic;
using System.Text.RegularExpressions;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    public class FoxMidtermCommands
    {
        public class GetSeatCounts : ICommand
        {
            public string CommandName { get; } = "Get seat counts";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"^midterms$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Gets the total number of senate, congressional and gubernatorial seats for each party";
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                var politicoData = Services.Politico.GetMidtermResults();
                return new List<string>
                {
                    $"House Results: {politicoData.House.Republicans.Total} Republicans - {politicoData.House.Democrats.Total} Democrats - {politicoData.House.Undecided} Undecided",
                    $"Senate Results: {politicoData.Senate.Republicans.Total} Republicans - {politicoData.Senate.Democrats.Total} Democrats - {politicoData.Senate.Undecided} Undecided"
                };
            }
        }
    }
}