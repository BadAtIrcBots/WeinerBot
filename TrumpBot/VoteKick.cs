using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace TrumpBot
{
    public class VoteKick
    {
        internal class Message
        {
            internal DateTime Time { get; set; }
        }

        internal class UserActivity
        {
            internal string Nick { get; set; }
            internal DateTime LastActivity { get; set; }
            internal List<Message> Activity { get; set; }
        }

        internal class ChannelActivity
        {
            internal string Channel { get; set; }
            internal List<UserActivity> UserActivities { get; set; }
        }

        public class VoteRecord
        {
            public string Channel { get; set; }
            public string Requestor { get; set; }
            public string Target { get; set; }
            public int VotesRequired { get; set; }
            public int VotesGotten { get; set; }
            public bool? VoteSuccessful { get; set; }
            public List<string> Voters { get; set; } 
        }

        internal class OngoingVote
        {
            internal string Channel { get; set; }
            internal string Requestor { get; set; }
            internal string Target { get; set; }
            internal int VotesRequired { get; set; }
            internal int VotesGotten { get; set; }
            internal List<string> Voters { get; set; } 
        }

        private List<ChannelActivity> _channelActivities = new List<ChannelActivity>();
        private List<VoteRecord> _voteRecords = new List<VoteRecord>(); 
        private List<OngoingVote> _ongoingVotes = new List<OngoingVote>();



        internal void StartVote(string target, string requestor, string channel)
        {
            int requiredVotes = _calculateRequiredVotes(channel);

            _voteRecords.Add(new VoteRecord
            {
                Channel = channel,
                Requestor = requestor,
                Target = target,
                Voters = new List<string> { requestor },
                VotesGotten = 0,
                VoteSuccessful = null,
                VotesRequired = requiredVotes
            });

            _ongoingVotes.Add(new OngoingVote
            {
                Channel = channel,
                Requestor = requestor,
                Target = target,
                Voters = new List<string> { requestor },
                VotesGotten = 0,
                VotesRequired = requiredVotes
            });
        }

        private int _calculateRequiredVotes(string channel)
        {
            ChannelActivity channelActivity =
                (from activeChannel in _channelActivities where activeChannel.Channel == channel select activeChannel).FirstOrDefault();

            if (channelActivity == null)
            {
                throw new Exception("Tried to calculate votes for a null channel");
            }

            int activeUsers = 0;
            foreach(UserActivity userActivity in channelActivity.UserActivities)
            {
                if (userActivity.LastActivity > (DateTime.UtcNow - new TimeSpan(0, 0, 30, 0)))
                {
                    continue;
                }

                activeUsers++;
            }

            if(activeUsers > 5) return activeUsers / 2;

            return activeUsers / 4;
        }

        internal void MessageReceived(string nick, string channel)
        {
            ChannelActivity channelActivity =
                (from activity in _channelActivities where activity.Channel == channel select activity).FirstOrDefault();

            if (channelActivity == null)
            {
                _channelActivities.Add(new ChannelActivity
                {
                    Channel = channel,
                    UserActivities = new List<UserActivity>
                    {
                        new UserActivity
                        {
                            Activity = new List<Message>
                            {
                                new Message
                                {
                                    Time = DateTime.UtcNow
                                }
                            },
                            Nick = nick,
                            LastActivity = DateTime.UtcNow
                        }
                    }
                });
                return;
            }

            UserActivity userActivity =
                                (from activity in channelActivity.UserActivities where activity.Nick == nick select activity)
                                    .FirstOrDefault();

            if (userActivity == null)
            {
                channelActivity.UserActivities.Add(new UserActivity
                {
                    Nick = nick,
                    LastActivity = DateTime.UtcNow,
                    Activity = new List<Message>
                    {
                        new Message
                        {
                            Time = DateTime.UtcNow
                        }
                    }
                });

                return;
            }

            userActivity.LastActivity = DateTime.UtcNow;
            userActivity.Activity.Add(new Message
            {
                Time = DateTime.UtcNow
            });
        }
    }
}
