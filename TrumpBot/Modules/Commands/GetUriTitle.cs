using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using TrumpBot.Models;

namespace TrumpBot.Modules.Commands
{
    [Command.NoPrefix]
    [Command.DoNotReportException]
    internal class GetUriTitle : ICommand
    {
        public string CommandName { get; } = "GetTitleForGivenUri";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Low;

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"(https?)://(-\.)?([^\s/?\.#]+\.?)+(/[^\s]*)?", RegexOptions.Compiled | RegexOptions.Multiline)
        };

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null,
            bool useCache = true)
        {
            var config = JsonConvert.DeserializeObject<ChannelUriConfig>(File.ReadAllText("Config\\uri.json"));
            List<string> enabledChannels = config.EnabledChannels;

            if (!enabledChannels.Contains(messageEvent.Channel)) return null;

            Uri matchedUri = new Uri(arguments[0].Value);

            string responseHtml;

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage headRequest = new HttpRequestMessage(HttpMethod.Head, matchedUri.AbsoluteUri);

                headRequest.Headers.UserAgent.ParseAdd(config.UserAgent);
                headRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                headRequest.Headers.Add("X-PX-AUTHORIZATION", "3:e76c2ea8a77ff1c04948d6d2df6775c4c4cdf92d1fac1686385531addd70b72f:uIOIhredLiWowZ3z8uzCCa9P1FMWQlnfxsWR3YLwS0x6iMJzL1WfWXjjYiIra+vfW1A/gbgL1Lh8Lsy8u1yJTQ==:1000:OaDoummliEwwdJgN3ZDkqVGf2KfVBR31tQxllxT2zKyvgo8H/A6RD6EZQS5yzPjN3aAo5dZn7IhcKGHsWFgI7JrFEJ6zy6GwDpMqgbnIV5aBECIZy17VtZZzDe92YlIY9KMfxfZXHDFdTqk1xDf+rY96FTIzFUuvAy0w3dZcqbc=");

                HttpResponseMessage headResponse = client.SendAsync(headRequest).Result;
                var contentLength = headResponse.Content.Headers.ContentLength ?? 0;
                var contentType = headResponse.Content.Headers.ContentType == null ? "text/html" : headResponse.Content.Headers.ContentType.MediaType;

                if (contentType != "text/html" ||
                    contentLength > (100 * 1024 * 1024))
                {
                    GC.Collect(); // GC for some reason doesn't flush until next request
                    if (headResponse.Content.Headers.ContentLength == null)
                    {
                        return new List<string>{$"[URL] {headResponse.Content.Headers.ContentType?.MediaType};{headResponse.Content.Headers.ContentType?.CharSet} No content length"};
                    }
                    return new List<string>{$"[URL] {headResponse.Content.Headers.ContentType?.MediaType};{headResponse.Content.Headers.ContentType?.CharSet} {headResponse.Content.Headers.ContentLength / 1024} KB"};
                }
            }


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
                client.DefaultRequestHeaders.Add("X-PX-AUTHORIZATION", "3:e76c2ea8a77ff1c04948d6d2df6775c4c4cdf92d1fac1686385531addd70b72f:uIOIhredLiWowZ3z8uzCCa9P1FMWQlnfxsWR3YLwS0x6iMJzL1WfWXjjYiIra+vfW1A/gbgL1Lh8Lsy8u1yJTQ==:1000:OaDoummliEwwdJgN3ZDkqVGf2KfVBR31tQxllxT2zKyvgo8H/A6RD6EZQS5yzPjN3aAo5dZn7IhcKGHsWFgI7JrFEJ6zy6GwDpMqgbnIV5aBECIZy17VtZZzDe92YlIY9KMfxfZXHDFdTqk1xDf+rY96FTIzFUuvAy0w3dZcqbc=");

                HttpResponseMessage response = client.GetAsync(matchedUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    responseHtml = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new Exception($"Bad HTTP status code, {response.StatusCode}");
                }
            }

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(responseHtml);

            if (matchedUri.Host.Contains("imgur"))
            {
                string imgurTitle = WebUtility.HtmlDecode(document.DocumentNode.SelectSingleNode("//meta[@property=\"og:title\"]")
                    .GetAttributeValue("content", "og:title missing"));
                if (document.DocumentNode.SelectSingleNode("//meta[@property=\"og:type\"]").GetAttributeValue("content", "og:type missing").ToLower()
                    .Contains("video.other"))
                {
                    return new List<string>
                    {
                        $"{imgurTitle} - {document.DocumentNode.SelectSingleNode("//meta[@name=\"twitter:player:stream\"]").GetAttributeValue("content", "twitter:player:stream missing")}"
                    };
                }
                if (document.DocumentNode.SelectSingleNode("//meta[@property=\"og:type\"]").GetAttributeValue("content", "og:type missing").ToLower()
                    .Contains("article"))
                {
                    return new List<string>
                    {
                        $"{imgurTitle} - {document.DocumentNode.SelectSingleNode("//meta[@name=\"twitter:image\"]").GetAttributeValue("content", "twitter:image missing")}"
                    };
                }
            }

            HtmlNode title = document.DocumentNode.SelectSingleNode("//title");
            if (title == null || title.InnerText == "") return null;

            var cleanTitle = title.InnerText;

            if (matchedUri.Host.Contains("twitter"))
            {
                cleanTitle = cleanTitle.Replace("&quot;", "");
            }

            return new List<string>
            {
                " " +
                WebUtility.HtmlDecode(cleanTitle)
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .TrimStart(' ')
            }; // Fuck you imgur for forcing me to do this shit
        }
    }

    internal class ChannelUriConfig
    {
        public List<string> EnabledChannels { get; set; } = new List<string>();
        public List<string> TwitterEnabledChannels { get; set; } = new List<string>();

        public string UserAgent { get; set; } =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:62.0) Gecko/20100101 Firefox/62.0";
    }
}