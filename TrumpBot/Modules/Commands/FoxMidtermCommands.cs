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
                var foxData = Services.FoxMidterms.GetMidtermData();
                return new List<string>
                {
                    $"Congress: {foxData.HousePredictions["1"].CurrentRepublicansTotal} Republicans, " +
                    $"{foxData.HousePredictions["1"].CurrentDemocratsTotal} Democrats and " +
                    $"{foxData.HousePredictions["1"].VacantSeatsTotal} vacant seats. " +
                    $"{foxData.HousePredictions["1"].SeatsNeededForControl} needed for control",
                    $"Senate: {foxData.SenatePredictions["1"].CurrentRepublicansTotal} Republicans and " +
                    $"{foxData.SenatePredictions["1"].CurrentDemocratsTotal} Democrats",
                    $"Gubernatorial: {foxData.GubernatorialPredictions["1"].CurrentRepublicansTotal} Republicans, " +
                    $"{foxData.GubernatorialPredictions["1"].CurrentDemocratsTotal} Democrats and " +
                    $"{foxData.GubernatorialPredictions["1"].CurrentIndependentsTotal} independent"
                };
            }
        }
    }
}