using System;
using System.Net;
using HtmlAgilityPack;
using log4net;
using SharpRaven.Data;

namespace TrumpBot.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static DateTime? ParseHtmlAttributeDateTime(this HtmlNode node, string xpath, string attribute)
        {
            var log = LogManager.GetLogger("ParseHtmlAttributeDateTime");
            try
            {
                return DateTime.Parse(WebUtility
                    .HtmlDecode(node.SelectSingleNode(xpath).GetAttributeValue(attribute, "no value"))
                    .ReplaceNewlines());
            }
            catch (Exception e)
            {
                Services.Raven.GetRavenClient()?.Capture(new SentryEvent(e));
                log.Error(e);
                return null;
            }
        }

        public static string ParseHtmlAttribute(this HtmlNode node, string xpath, string attribute,
            string defaultValue = "")
        {
            var log = LogManager.GetLogger("ParseHtmlAttribute");
            try
            {
                return WebUtility.HtmlDecode(node.SelectSingleNode(xpath).GetAttributeValue(attribute, defaultValue))
                    .ReplaceNewlines();
            }
            catch (Exception e)
            {
                Services.Raven.GetRavenClient()?.Capture(new SentryEvent(e));
                log.Error(e);
                return defaultValue;
            }
        }

        public static string ParseHtmlElement(this HtmlNode node, string xpath)
        {
            var log = LogManager.GetLogger("ParseHtmlElement");
            try
            {
                return WebUtility.HtmlDecode(node.SelectSingleNode(xpath).InnerText.ReplaceNewlines());
            }
            catch (Exception e)
            {
                Services.Raven.GetRavenClient()?.Capture(new SentryEvent(e));
                log.Error(e);
                return null;
            }
        }
    }
}