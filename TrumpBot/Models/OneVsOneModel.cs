using System;
using System.Collections.Generic;

namespace TrumpBot.Models
{
    public class OneVsOneModel
    {
        public class OneVsOne
        {
            public string Slug { get; set; }
            public List<string> FirstCandidate { get; set; } 
            public List<string> SecondCandidate { get; set; }
            public string Name { get; set; }
            public Uri ShortUri { get; set; }
        }

    }
}
