using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    public class YouTubeCommand
    {
        [Command.NoPrefix]
        [Command.BreakAfterExecution]
        internal class GetYouTubeTitleByUri : ICommand
        {
            public string CommandName { get; } = "Get YouTube Title By URI";
            public List<Regex> Patterns { get; set; } = new List<Regex>
            {
                new Regex(@"(?:youtube.*?(?:v=|\/v\/)|youtu\.be\/|yooouuutuuube.*?id=)([-_a-zA-Z0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline),
                new Regex(@"https?:\/\/hooktube\.com\/watch\?v=([-_a-zA-Z0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)
            };
            public bool HideFromHelp { get; set; } = false;
            public string HelpDescription { get; set; } = "Fetches YouTube video metdata by looking for the video ID in the URL.";
            public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
            public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
            {
                var enabledChannels = ConfigHelpers.LoadConfig<ChannelUriConfigModel>(ConfigHelpers.ConfigPaths.ChannelUriConfig).YouTubeEnabledChannels;
                if (!enabledChannels.Contains(messageEvent.Channel)) return null;
                string videoId = arguments[1].Value;
                YouTubeApiConfigModel config =
                    ConfigHelpers.LoadConfig<YouTubeApiConfigModel>(ConfigHelpers.ConfigPaths.YouTubeApiConfig);
                YouTubeApiModels.ContentDetailsRoot video =
                    JsonConvert.DeserializeObject<YouTubeApiModels.ContentDetailsRoot>(
                        Http.GetJson(new Uri(
                            $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails%2C+snippet%2C+statistics&id={videoId}&key={config.ApiKey}")));
                if (video.PageInfo.TotalResults == 0)
                {
                    return new List<string>{"Video not found"};
                }

                var item = video.Items[0];
                return new List<string>{$"{item.Snippet.Title} | {item.Snippet.ChannelTitle} - https://www.youtube.com/watch?v={item.Id}"};
            }
        }
    }
}