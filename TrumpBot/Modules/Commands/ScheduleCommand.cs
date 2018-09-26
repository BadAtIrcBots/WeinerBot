using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Humanizer;
using TrumpBot.Models;
using TrumpBot.Services;
using Tweetinvi.Core.Extensions;
using Calendar = Ical.Net.Calendar;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(300)]
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
            string scheduleHtml = Http.Get(scheduleUri, fuzzUserAgent: true, compression: true, timeout: 20000);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(scheduleHtml);

            List<Calendar> calendars = new List<Calendar>();
            foreach (var scheduledEvent in document.DocumentNode.SelectNodes("//a[@class=\"add-to-calendar\"]"))
            {
                string icalText =
                    Http.Get(new Uri("https://www.donaldjtrump.com" + scheduledEvent.GetAttributeValue("href", "href missing from attribute")),
                        fuzzUserAgent: true, compression: true, timeout: 20000);
                calendars.Add(Calendar.Load(icalText));
            }
            
            List<string> result = new List<string>();
            foreach (var calendar in calendars.Take(5).OrderBy(c => c.Events.First().DtEnd))
            {
                string location = calendar.Events.First().Location;
                string tz = string.Empty;
                string shortTz = string.Empty;
                string relativeTime = string.Empty;
                if (location.Contains("ET") || location.Contains("EDT") ||
                    location.Contains("EST") || location.ToLower().Contains("eastern time"))
                {
                    tz = "Eastern Standard Time"; // Even though it says standard, Windows will figure out DST for us! MAGIC
                    shortTz = " ET";
                }
                else if (location.Contains("CT") || location.Contains("CDT") ||
                         location.Contains("CST") || location.ToLower().Contains("central time"))
                {
                    tz = "Central Standard Time";
                    shortTz = " CT";
                }
                else if (location.Contains("PT") || location.Contains("PDT") ||
                         location.Contains("PST") || location.ToLower().Contains("pacific time"))
                {
                    tz = "Pacific Standard Time";
                    shortTz = " PT";
                }
                else if (location.Contains("MT") || location.Contains("MDT") ||
                         location.Contains("MST") || location.ToLower().Contains("mountain time"))
                {
                    tz = "Mountain Standard Time";
                    shortTz = " MT";
                }
                
                if (!tz.IsNullOrEmpty())
                {
                    relativeTime = " (" +
                                   (calendar.Events.First().DtStart.Value -
                                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                        TimeZoneInfo.FindSystemTimeZoneById(tz)))
                                   .Humanize()
                                   + ")";
                }
                result.Add($"{calendar.Events.First().DtStart.Value:yyyy-MM-dd hh:mm tt}{shortTz}: {calendar.Events.First().Summary}: {location.Replace("\n", " ")}{relativeTime}");
            }

            return result;
        }
    }
}