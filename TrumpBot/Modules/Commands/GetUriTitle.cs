using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Humanizer;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Models;
using TrumpBot.Models.Config;

namespace TrumpBot.Modules.Commands
{
    [Command.NoPrefix]
    [Command.DoNotReportException]
    internal class GetUriTitle : ICommand
    {
        public string CommandName { get; } = "Get Title For Given URI";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Low;

        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"(https?)://(-\.)?([^\s/?\.#]+\.?)+(/[^\s]*)?", RegexOptions.Compiled | RegexOptions.Multiline)
        };
        public bool HideFromHelp { get; set; } = false;
        public string HelpDescription { get; set; } = "Matches links posted to the chat and fetches metadata such as the title and description of the page or size and content type if it is not a text/html response.";

        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null,
            bool useCache = true)
        {
            var config = ConfigHelpers.LoadConfig<ChannelUriConfigModel>(ConfigHelpers.ConfigPaths.ChannelUriConfig);
            List<string> enabledChannels = config.EnabledChannels;

            if (!enabledChannels.Contains(messageEvent.Channel)) return null;

            Uri matchedUri = new Uri(arguments[0].Value);

            string responseHtml;

            using (HttpClient client = new HttpClient(new HttpClientHandler {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate}))
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


            using (HttpClient client = new HttpClient(new HttpClientHandler {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate}))
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

            string description = null;
            bool fetchDescription = true;
            foreach (var domain in config.DomainsToIgnoreDescriptions)
            {
                if (matchedUri.Host.Contains(domain))
                {
                    fetchDescription = false;
                }
            }

            if (fetchDescription)
            {
                try
                {
                    try
                    {
                        description = WebUtility.HtmlDecode(document.DocumentNode
                                .SelectSingleNode("//meta[@name=\"description\"]")
                                .GetAttributeValue("content", "no description"))
                            .Replace("\n", string.Empty).Replace("\r", string.Empty);
                    }
                    catch (NullReferenceException e)
                    {
                        description = "no description";
                    }

                    if (description == "no description")
                    {
                        description = WebUtility.HtmlDecode(document.DocumentNode
                                .SelectSingleNode("//meta[@property=\"og:description\"]")
                                .GetAttributeValue("content", "no description"))
                            .Replace("\n", string.Empty).Replace("\r", string.Empty);
                    }
                }
                catch (Exception e)
                {
                    Services.Raven.GetRavenClient()?.Capture(new SentryEvent(e));
                }
            }


            if (description == "no description") description = null;

            return new List<string>
            {
                WebUtility.HtmlDecode(cleanTitle)
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .TrimStart(' '),
                description?.TrimStart(' ').Truncate(400)
            };
        }
    }
}