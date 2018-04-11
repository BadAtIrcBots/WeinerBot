using System;

namespace TrumpBot.Models
{
    public class DebateModel : BaseModel
    {
        public class Debate
        {
            public DateTimeOffset Date { get; set; }
            public string Location { get; set; }
            public string Host { get; set; }
            public bool TimeKnown { get; set; }
        }
    }
}
