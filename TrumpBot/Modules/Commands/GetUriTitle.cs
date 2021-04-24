using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Humanizer;
using SharpRaven.Data;
using TrumpBot.Configs;
using TrumpBot.Extensions;
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
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
                client.MaxResponseContentBufferSize = 100000000;
                client.DefaultRequestHeaders.Add("X-PX-AUTHORIZATION", "3:e76c2ea8a77ff1c04948d6d2df6775c4c4cdf92d1fac1686385531addd70b72f:uIOIhredLiWowZ3z8uzCCa9P1FMWQlnfxsWR3YLwS0x6iMJzL1WfWXjjYiIra+vfW1A/gbgL1Lh8Lsy8u1yJTQ==:1000:OaDoummliEwwdJgN3ZDkqVGf2KfVBR31tQxllxT2zKyvgo8H/A6RD6EZQS5yzPjN3aAo5dZn7IhcKGHsWFgI7JrFEJ6zy6GwDpMqgbnIV5aBECIZy17VtZZzDe92YlIY9KMfxfZXHDFdTqk1xDf+rY96FTIzFUuvAy0w3dZcqbc=");

                HttpResponseMessage response = client.GetAsync(matchedUri).Result;
                
                var contentLength = response.Content.Headers.ContentLength ?? 0;
                var contentType = response.Content.Headers.ContentType == null ? "text/html" : response.Content.Headers.ContentType.MediaType;

                if (contentType != "text/html" ||
                    contentLength > (100 * 1024 * 1024))
                {
                    GC.Collect(); // GC for some reason doesn't flush until next request
                    if (response.Content.Headers.ContentLength == null)
                    {
                        return new List<string>{$"[URL] {response.Content.Headers.ContentType?.MediaType};{response.Content.Headers.ContentType?.CharSet} No content length"};
                    }
                    return new List<string>{$"[URL] {response.Content.Headers.ContentType?.MediaType};{response.Content.Headers.ContentType?.CharSet} {contentLength / 1024} KB"};
                }
                
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
                string imgurTitle = document.DocumentNode.ParseHtmlAttribute("//meta[@property=\"og:title\"]",
                    "content", "og:title missing");
                if (document.DocumentNode.ParseHtmlAttribute("//meta[@property=\"og:type\"]", "content", "og:type missing").ToLower()
                    .Contains("video.other"))
                {
                    return new List<string>
                    {
                        $"{imgurTitle} - {document.DocumentNode.ParseHtmlAttribute("//meta[@name=\"twitter:player:stream\"]", "content", "twitter:player:stream missing")}"
                    };
                }

                if (document.DocumentNode
                    .ParseHtmlAttribute("//meta[@property=\"og:type\"]", "content", "og:type missing").ToLower()
                    .Contains("article"))
                {
                    return new List<string>
                    {
                        $"{imgurTitle} - {document.DocumentNode.ParseHtmlAttribute("//meta[@name=\"twitter:image\"]", "content", "twitter:image missing")}"
                    };
                }
            }

            var title = document.DocumentNode.ParseHtmlElement("//title");
            if (title == null) return null;

            string description = null;
            bool fetchDescription = true;
            foreach (var domain in
                config.DomainsToIgnoreDescriptions.Where(domain => matchedUri.Host.Contains(domain)))
            {
                fetchDescription = false;
            }

            if (fetchDescription)
            {
                description =
                    document.DocumentNode.ParseHtmlAttribute("//meta[@name=\"description\"]", "content",
                        "no description");

                var ogDescription = document.DocumentNode.ParseHtmlAttribute("//meta[@property=\"og:description\"]",
                    "content", "no description");

                if (description == "no description" || ogDescription.Length > description.Length)
                {
                    description = ogDescription;
                }
            }

            DateTime? createTime = null;
            DateTime? modifyTime = null;
            if (config.AppendMetaDates)
            {
                createTime =
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@property=\"article:published_time\"]", "content") ?? 
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@name=\"article:published_time\"]", "content") ?? 
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@name=\"article.published\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@itemprop=\"dateCreated\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@itemprop=\"datePublished\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@name=\"article.created\"]", "content");

                modifyTime =
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@property=\"article:modified_time\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@name=\"article:modified_time\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@itemprop=\"dateModified\"]", "content") ??
                    document.DocumentNode.ParseHtmlAttributeDateTime("//meta[@name=\"article.updated\"]", "content");
            }

            bool appendModifyTime = true;

            if (createTime != null)
            {
                title += $" (published {createTime.Humanize(false, DateTime.Now)}";
                if (modifyTime != null)
                {
                    if (modifyTime.Humanize(false, DateTime.Now) == createTime.Humanize(false, DateTime.Now))
                    {
                        title += ")";
                        appendModifyTime = false;
                    }
                    else
                    {
                        title += ", ";
                    }
                }
                else
                {
                    title += ")";
                }
            }

            if (modifyTime != null && appendModifyTime)
            {
                
                title += $"modified {modifyTime.Humanize(false, DateTime.Now)})";
            }

            if (description == "no description") description = null;
            
            List<string> result = new List<string>
            {
                title
                .TrimStart(' ')
            };
            if (description != null && description != result[0])
            {
                result.Add(description.TrimStart(' ').Truncate(400));
            }

            return result;
        }
    }
}