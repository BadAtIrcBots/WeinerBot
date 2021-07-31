using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Backtrace;
using NLog;
using Meebey.SmartIrc4net;
using WeinerBot.Extensions;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using Tweetinvi.Streaming.Parameters;
using WeinerBot.Configs;
using WeinerBot.Models.Config;

namespace WeinerBot.Modules
{
    internal class TwitterStream
    {
        private readonly IrcClient _ircClient;
        private readonly Thread _thread;
        private TwitterStreamConfigModel.StreamConfig _config;
        private TwitterClient _twitterClient;
        internal IFilteredStream FilteredStream;
        private Logger _log = LogManager.GetCurrentClassLogger();
        private string _breadcrumbName = "TwitterStream Thread";
        private BacktraceClient _backtraceClient = Services.Backtrace.GetBacktraceClient();

        internal TwitterStream(IrcClient client)
        {
            _ircClient = client;
            
            LoadConfig();
            if (!_config.StreamEnabled)
            {
                // Fuck you and the fucking pile of piss your shitty application came out of. Fuck your shitty API for suddenly not working and causing annoying to trace errors
                // Come over here and suck my cock @Jack you piece of shit.
                return;
            }
            
            _twitterClient = Services.Twitter.GetTwitterClient();

            _backtraceClient?.Send("Authenticating to Twitter");
            IAuthenticatedUser authedUser;

            try
            {
                authedUser = _twitterClient.Users.GetAuthenticatedUserAsync().Result;
            }
            catch (TwitterAuthException e)
            {
                _log.Error("When attempting to authenticate with Twitter, got exception:");
                _log.Error(e);
                _log.Error("Self destructing the Tweet thread");
                _backtraceClient?.Send(e);
                return;
            }
            catch (TwitterException e)
            {
                _log.Error("Got TwitterException when testing creds");
                _log.Error(e);
                _backtraceClient?.Send(e);
                return;
            }
            catch (Exception e)
            {
                _log.Error("Got an exception when testing creds");
                _log.Error(e);
                _backtraceClient?.Send(e);
                return;
            }
            
            _log.Info($"Auth'd as {authedUser.Name}");

            _thread = new Thread(() => TweetThread());
            _thread.Start();
        }

        internal void LoadConfig()
        {
            _config = ConfigHelpers.LoadConfig<TwitterStreamConfigModel.StreamConfig>(ConfigHelpers.ConfigPaths
                .TwitterStreamConfig);
            _log.Info("TwitterStream has loaded its config");
        }

        private void TweetThread()
        {
            _backtraceClient?.Send($"{_breadcrumbName} started");

            _log.Info("Tweet thread started!");

            FilteredStream = _twitterClient.Streams.CreateFilteredStream();
            FilteredStream.FilterLevel = StreamFilterLevel.None;
            FilteredStream.StallWarnings = true;

            foreach (var stream in _config.Streams)
            {
                FilteredStream.AddFollow(stream.TwitterUserId);
                _log.Info($"Added {stream.TwitterUserId} to tracking");
            }
            
            _backtraceClient?.Send("Added configured IDs to stream");

            FilteredStream.MatchingTweetReceived += (sender, args) =>
            {
                ITweet tweet = args.Tweet;
                _log.Info($"Found tweet from {tweet.CreatedBy.ScreenName}");

                TwitterStreamConfigModel.Stream stream =
                    _config.Streams.Find(s => s.TwitterUserId == tweet.CreatedBy.Id);

                if (stream == null)
                {
                    return;
                }
                Dictionary<string, string> data =
                    new Dictionary<string, string>
                    {
                        {"Twitter ID", tweet.CreatedBy.IdStr},
                        {"Tweet ID", tweet.IdStr},
                        {"Tweet Body", tweet.FullText},
                        {"Destination channels", string.Join(", ", stream.Channels)}
                    };

                _backtraceClient?.Send($"Found matching Tweet, from {tweet.CreatedBy.Name}");

                if (stream.IgnoreRetweets && tweet.IsRetweet)
                {
                    _log.Info(
                        $"Ignoring tweet {tweet.IdStr} as IgnoreRetweets is {stream.IgnoreRetweets} and IsRetweet is {tweet.IsRetweet}");
                    return;
                }

                if (stream.IgnoreReplies && tweet.InReplyToUserId != null)
                {
                    _log.Info(
                        $"Ignoring tweet {tweet.IdStr} as IgnoreReplies is {stream.IgnoreReplies} and InReplyToUserId is not null (it is {tweet.InReplyToUserId})");
                    return;
                }

                if (tweet.IsRetweet)
                {
                    if (stream.RetweetsToIgnoreByUserId.Contains(tweet.RetweetedTweet.CreatedBy.Id))
                    {
                        _log.Info($"Ignoring tweet {tweet.IdStr} as author has been ignored");
                        return;
                    }
                }

                foreach (string channel in stream.Channels)
                {
                    if (!_ircClient.JoinedChannels.Contains(channel)) continue;
                    
                    _log.Info($"Sending tweet from {tweet.CreatedBy.Name} to {channel}");
                    if (_ircClient.IsConnected)
                    {
                        if (tweet.IsRetweet)
                        {
                            if (tweet.CreatedBy.Name == tweet.CreatedBy.ScreenName)
                            {
                                _ircClient.SendMessage(SendType.Message, channel,
                                    $"{IrcConstants.IrcBold}@{tweet.CreatedBy.ScreenName}:{IrcConstants.IrcNormal} RT @{tweet.RetweetedTweet.CreatedBy.ScreenName} {WebUtility.HtmlDecode(tweet.RetweetedTweet.FullText.ReplaceNonPrintableCharacters(' ').ReplaceNewlines("⏎"))} - {tweet.Url}");
                                return;
                            }
                            _ircClient.SendMessage(SendType.Message, channel,
                                $"{IrcConstants.IrcBold}{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName}):{IrcConstants.IrcNormal} RT @{tweet.RetweetedTweet.CreatedBy.ScreenName} {WebUtility.HtmlDecode(tweet.RetweetedTweet.FullText.ReplaceNonPrintableCharacters(' ').ReplaceNewlines("⏎"))} - {tweet.Url}");
                            return;
                        }

                        if (tweet.CreatedBy.Name == tweet.CreatedBy.ScreenName)
                        {
                            _ircClient.SendMessage(SendType.Message, channel,
                                $"{IrcConstants.IrcBold}@{tweet.CreatedBy.ScreenName}:{IrcConstants.IrcNormal} {WebUtility.HtmlDecode(tweet.FullText.ReplaceNonPrintableCharacters(' ').ReplaceNewlines("⏎"))} - {tweet.Url}");
                            return;
                        }
                        _ircClient.SendMessage(SendType.Message, channel,
                            $"{IrcConstants.IrcBold}{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName}):{IrcConstants.IrcNormal} {WebUtility.HtmlDecode(tweet.FullText.ReplaceNonPrintableCharacters(' ').ReplaceNewlines("⏎"))} - {tweet.Url}");
                        return;
                    }
                    _log.Error("Tried to send message to channel but IRC bot is not connected");
                }
            };

            FilteredStream.StreamStopped += (sender, args) =>
            {
                _log.Error("Twitter stream disconnected with following reason");
                _log.Error(args.DisconnectMessage?.Reason);
                _backtraceClient?.Send(args.Exception);
                if (args.DisconnectMessage != null) // If socket closed for "reasons" this will be null
                {
                    _log.Error(
                        $"Twitter disconnect message was: ({args.DisconnectMessage.Code}) {args.DisconnectMessage.Reason}");
                }
            };

            FilteredStream.WarningFallingBehindDetected += (sender, args) =>
            {
                _log.Info($"Twitter stream is falling behind. Warning from Twitter: {args.WarningMessage.Message}");
                _log.Info($"Twitter queue is {args.WarningMessage.PercentFull}% full");
                _backtraceClient?.Send($"Twitter stream falling behind, queue {args.WarningMessage.PercentFull}% full");
            };

            while (true)
            {
                try
                {
                    FilteredStream.StartMatchingAnyConditionAsync().Wait();
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
    }
}