using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TrumpBot.Models;
using TrumpBot.Services;
using Calendar = Ical.Net.Calendar;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(600)]
    public class ScheduleCommand : ICommand
    {
        public string CommandName { get; } = "schedule";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^schedule$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^rallies$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^rally$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null,
            bool useCache = true)
        {
            Uri scheduleUri = new Uri("https://www.donaldjtrump.com/rallies/");
            string scheduleHtml = Http.Get(scheduleUri, fuzzUserAgent: true, compression: true);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(scheduleHtml);

            List<Calendar> calendars = new List<Calendar>();
            foreach (var scheduledEvent in document.DocumentNode.SelectNodes("//a[@class=\"add-to-calendar\"]"))
            {
                string icalText =
                    Http.Get(new Uri("https://www.donaldjtrump.com" + scheduledEvent.GetAttributeValue("href", "href missing from attribute")),
                        fuzzUserAgent: true, compression: true);
                calendars.Add(Calendar.Load(icalText));
            }
            
            List<string> result = new List<string>();
            foreach (var calendar in calendars.Take(3).OrderBy(c => c.Events.First().DtEnd))
            {
                result.Add($"{calendar.Events.First().DtStart.Value:yyyy-MM-dd}: {calendar.Events.First().Summary}: {calendar.Events.First().Location.Replace("\n", " ")}");
            }

            return result;
        }
    }
}