using System.Collections.Generic;

namespace TrumpBot.Models.Config
{
    public class RcpConfigModel
    {
        public class RcpConfigRoot
        {
            public List<RcpPoll> RcpPolls { get; set; }
        }

        public class RcpPoll
        {
            public string Name { get; set; } // Identifier for this when a user tries to query for it, !rcp <Name>
            public int Id { get; set; } // ID from the URL, like 6185 from https://www.realclearpolitics.com/epolls/other/2018_generic_congressional_vote-6185.html
        }
    }
}