using Meebey.SmartIrc4net;
using TrumpBot.Models;

namespace TrumpBot.Extensions
{
    public static class IrcMessageDataExtensions
    {
        public static ChannelMessageEventDataModel CastToIrcChannelMessageEventData(this IrcMessageData messageData)
        {
            return new ChannelMessageEventDataModel
            {
                From = messageData.From,
                Nick = messageData.Nick.GetNick(),
                Ident = messageData.Ident,
                Host = messageData.Host,
                Channel = messageData.Channel.ToLower(),
                MessageWithPrefix = messageData.Message,
                RawMessage = messageData.RawMessage
            };
        }
    }
}