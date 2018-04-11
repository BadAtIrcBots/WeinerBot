using System;
using System.Collections.Generic;

namespace TrumpBot.Models
{
    public class PollModel
    {
        public class Poll
        {
            public string Slug { get; set; }
            public List<string> FriendlyNames { get; set; } // List of user friendly names for the poll, not case sensitive
            public string Name { get; set; } = "(Some idiot didn't fill this in)"; // Should be something which a friendly name would match, since this is what is displayed when polls are listed in help
            public Uri ShortUri { get; set; }
        }
    }
}
