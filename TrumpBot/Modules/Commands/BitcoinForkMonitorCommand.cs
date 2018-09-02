using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TrumpBot.Extensions;
using TrumpBot.Models;
using TrumpBot.Services;

namespace TrumpBot.Modules.Commands
{
    [Command.CacheOutput(60)]
    internal class BitcoinForkMonitorCommand : ICommand
    {
        public string CommandName { get; } = "BitcoinForkMonitor";
        public Command.CommandPriority Priority { get; set; } = Command.CommandPriority.Normal;
        public List<Regex> Patterns { get; set; } = new List<Regex>
        {
            new Regex(@"^fork$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"^fork (\w+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        public List<string> RunCommand(ChannelMessageEventDataModel messageEvent, GroupCollection arguments = null, bool useCache = true)
        {
            bool checkGenericChainSplit = arguments != null && arguments.Count == 1;
            Uri forkMonUri = new Uri("https://www.btcforkmonitor.info/");
            string html;
            HtmlDocument forkMonitorDocument = new HtmlDocument();

            if (checkGenericChainSplit)
            {
                html = Http.Get(forkMonUri, fuzzUserAgent: true, timeout: 2000);
                forkMonitorDocument.LoadHtml(html);
                HtmlNode alertNode = forkMonitorDocument.DocumentNode.SelectSingleNode("//div[@align=\"center\"]");
                if (alertNode == null)
                {
                    throw new Exception("Could not find alert node");
                }
                return new List<string>{alertNode.InnerText.Replace("\n", "").TrimStart().TrimEnd(), "More options: core, bcc, bip148 (or uasf), btc1 (or segwit2x), segsignal"};
            }

            string argument;

            if (arguments != null && arguments.Count == 2)
            {
                argument = arguments[1].Value.ToLower(); // Tbh shouldn't really be necessary but you never know.
            }
            else
            {
                throw new Exception("arguments were null");
            }

            if (arguments.Count == 2)
            {
                if (argument != "bcc" && argument != "bip148" && argument != "uasf" && argument != "btc1" &&
                    argument != "segwit2x" && argument != "segsignal" && argument != "core")
                {
                    return "Did not recognise argument".SplitInParts(430).ToList();
                }
            }

            html = Http.Get(forkMonUri, fuzzUserAgent: true, timeout: 2000);
            forkMonitorDocument.LoadHtml(html);

            foreach (HtmlNode cardNode in forkMonitorDocument.DocumentNode.SelectNodes("//div[contains(@class, \"card\")]"))
            {
                string title = "";
                string blockchainReorganisation = "No value";
                string forkedText = "No value";
                
                if (!cardNode.HasChildNodes)
                {
                    throw new Exception("Card node had no child nodes.");
                }
                foreach (HtmlNode childNode in cardNode.ChildNodes)
                {
                    if (childNode.GetAttributeValue("class", "novalue").Contains("card-header"))
                    {
                        title = childNode.InnerText.Replace("Node: ", string.Empty);
                    }
                    else if (childNode.GetAttributeValue("class", "novalue").Contains("list-group"))
                    {
                        if (!childNode.HasChildNodes)
                        {
                            throw new Exception("list-group had no child nodes.");
                        }
                        foreach (HtmlNode listNode in childNode.ChildNodes)
                        {
                            if (listNode.InnerText.Contains("Blockchain Reorganization"))
                            {
                                blockchainReorganisation = listNode.InnerText;
                            }
                            else if (listNode.InnerText.Contains("Has not forked but is behind other nodes:"))
                            {
                                forkedText = listNode.InnerText;
                            }
                        }

                    }
                }

                // smartirc4net is not a fan of \r\n
                title = title.Replace("\n", " ").Replace("\r", " ").TrimStart().TrimEnd().RemoveMultipleSpaces();
                blockchainReorganisation = blockchainReorganisation.Replace("\n", " ").Replace("\r", " ").TrimStart().TrimEnd().RemoveMultipleSpaces();
                forkedText = forkedText.Replace("\n", " ").Replace("\r", " ").TrimStart().TrimEnd().RemoveMultipleSpaces();

                if (argument == "core" && title == "Bitcoin Core")
                {
                    return new List<string>
                    {
                        title,
                        blockchainReorganisation,
                        forkedText
                    };
                }
                if ((argument == "bip148" || argument == "uasf") && title == "UASF (BIP 148)")
                {
                    return new List<string>
                    {
                        title,
                        blockchainReorganisation,
                        forkedText
                    };
                }
                if ((argument == "btc1" || argument == "segwit2x") && title == "btc1 (segwit2x)")
                {
                    return new List<string>
                    {
                        title,
                        blockchainReorganisation,
                        forkedText
                    };
                }
                if (argument == "bcc" && title == "Bitcoin ABC (Bitcoin Cash; Bcash)")
                {
                    return new List<string>
                    {
                        title,
                        blockchainReorganisation,
                        forkedText
                    };
                }
                if (argument == "segsignal" && title == "Segsignal (BIP 91 only)")
                {
                    return new List<string>
                    {
                        title,
                        blockchainReorganisation,
                        forkedText
                    };
                }
            }

            return "Argument and title pair never matched up".SplitInParts(430).ToList();
        }
    }
}
