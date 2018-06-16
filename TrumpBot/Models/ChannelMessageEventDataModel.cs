namespace TrumpBot.Models
{
    public class ChannelMessageEventDataModel
    {
        public string From { get; set; }
        public string Nick { get; set; }
        public string Ident { get; set; }
        public string Host { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
        public string MessageWithPrefix { get; set; }
        public string RawMessage { get; set; }
    }
}