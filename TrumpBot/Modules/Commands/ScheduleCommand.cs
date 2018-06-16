using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(600)]
    public class ScheduleCommand : ICommand
    {
        public string CommandName { get; } = "schedule";

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^schedule$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null,
            bool useCache = true)
        {
            Uri scheduleUri = new Uri("http://www.donaldjtrump.com/schedule/");
            string scheduleHtml;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
                    HttpResponseMessage response = client.GetAsync(scheduleUri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        scheduleHtml = response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        throw new Exception($"Bad HTTP status code, {response.StatusCode}");
                    }
                }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(scheduleHtml);

            const string dateTimeFormat = "- dddd, MMMM d, yyyy -h:mm tt";

            List<ScheduleData> events = new List<ScheduleData>();
            try
            {
                foreach (var scheduledEvent in document.DocumentNode.SelectNodes("//div[@class=\"event_item\"]"))
                {
                    string date =
                        (from node in scheduledEvent.ChildNodes where node.Name == "h6" select node).FirstOrDefault()
                        .InnerText;
                    string time =
                        (from node in scheduledEvent.ChildNodes where node.Name == "h5" select node).FirstOrDefault()
                        .InnerText;
                    string location =
                        (from node in scheduledEvent.ChildNodes where node.Name == "p" select node).FirstOrDefault()
                        .InnerText;
                    string state =
                        (from node in scheduledEvent.ChildNodes where node.Name == "h2" select node).FirstOrDefault()
                        .InnerText;
                    events.Add(new ScheduleData
                    {
                        EventTime = DateTime.ParseExact(date + time, dateTimeFormat, null),
                        State = state,
                        Location = location
                    });
                }
            }
            catch (NullReferenceException)
            {
                return new List<string>
                {
                    $"No events are currently scheduled, see {scheduleUri.AbsoluteUri} for more information"
                };
            }


            string result = "Scheduled trump events:";
            result = events.Aggregate(result,
                (current, selectedEvent) =>
                    current +
                    $" {selectedEvent.EventTime:MMMM dd hh:mm tt} at {selectedEvent.Location}, {selectedEvent.State};");
            result += $" See {scheduleUri.AbsoluteUri} for more information";

            return new List<string> {result};
        }
    }

    public class ScheduleData
    {
        public DateTime EventTime { get; set; }
        public string State { get; set; }
        public string Location { get; set; }
    }
}