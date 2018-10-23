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
    public class ScheduleCommand : ICommand
    {
        public string CommandName { get; } = "Get DJT Rally Schedule";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Fetches the DonaldJTrump.com rally schedule, if you run schedule or rallies, it is limited to 5, if you run rally it'll only return the next rally.";

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
            string scheduleHtml = Cache.Get<string>(scheduleUri.AbsoluteUri);
            if (scheduleHtml == null)
            {
                scheduleHtml = Http.Get(scheduleUri, fuzzUserAgent: true, compression: true, timeout: 20000);
                Cache.Set(scheduleUri.AbsoluteUri, scheduleHtml, DateTimeOffset.Now.AddMinutes(15));
            }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(scheduleHtml);
            List<string> result = new List<string>();
            int limit = 5;
            if (messageEvent.Message.ToLower().StartsWith("rally"))
            {
                limit = 1;
            }

            int count = 0;
            foreach (var scheduledEvent in document.DocumentNode.SelectNodes("//a[@class=\"add-to-calendar\"]"))
            {
                count++;
                if (count > limit) break;
                string icalPath = scheduledEvent.GetAttributeValue("href", "href missing from attribute");
                string icalText = Cache.Get<string>(icalPath);
                if (icalText == null)
                {
                    icalText =
                        Http.Get(
                            new Uri("https://www.donaldjtrump.com" + icalPath),
                            fuzzUserAgent: true, compression: true, timeout: 20000);
                    Cache.Set(icalPath, icalText, DateTimeOffset.Now.AddMinutes(60));
                }

                var calendar = Calendar.Load(icalText);
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
                                   calendar.Events.First().DtStart.Value.Humanize(
                                       dateToCompareAgainst: TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                           TimeZoneInfo.FindSystemTimeZoneById(tz)))
                                   + ")";
                }

                result.Add(
                    $"{calendar.Events.First().DtStart.Value:yyyy-MM-dd hh:mm tt}{shortTz}: {calendar.Events.First().Summary}: {location.Replace("\n", " ")}{relativeTime}");
            }

            return result;
        }
    }
}