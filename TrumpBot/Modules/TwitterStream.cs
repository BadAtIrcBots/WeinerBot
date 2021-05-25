﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Backtrace;
using log4net;
using Meebey.SmartIrc4net;
using TrumpBot.Configs;
using TrumpBot.Models.Config;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using Tweetinvi.Streaming.Parameters;

namespace TrumpBot.Modules
{
    internal class TwitterStream
    {
        private readonly IrcClient _ircClient;
        private readonly Thread _thread;
        private TwitterStreamConfigModel.StreamConfig _config;
        private TwitterClient _twitterClient;
        internal IFilteredStream FilteredStream;
        private ILog _log = LogManager.GetLogger(typeof(TwitterStream));
        private string _breadcrumbName = "TwitterStream Thread";
        private BacktraceClient _backtraceClient = Services.Backtrace.GetBacktraceClient();

        internal TwitterStream(IrcClient client)
        {
            _ircClient = client;
            
            LoadConfig();
            if (!_config.Enabled)
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
            
            _log.Debug($"Auth'd as {authedUser.Name}");

            _thread = new Thread(() => TweetThread());
            _thread.Start();
        }

        internal void LoadConfig()
        {
            _config = ConfigHelpers.LoadConfig<TwitterStreamConfigModel.StreamConfig>(ConfigHelpers.ConfigPaths
                .TwitterStreamConfig);
            _log.Debug("TwitterStream has loaded its config");
        }

        private void TweetThread()
        {
            _backtraceClient?.Send($"{_breadcrumbName} started");

            _log.Debug("Tweet thread started!");

            if (!_config.StreamEnabled)
            {
                return;
            }

            FilteredStream = _twitterClient.Streams.CreateFilteredStream();
            FilteredStream.FilterLevel = StreamFilterLevel.None;
            FilteredStream.StallWarnings = true;

            foreach (var stream in _config.Streams)
            {
                FilteredStream.AddFollow(stream.TwitterUserId);
                _log.Debug($"Added {stream.TwitterUserId} to tracking");
            }
            
            _backtraceClient?.Send("Added configured IDs to stream");

            FilteredStream.MatchingTweetReceived += (sender, args) =>
            {
                ITweet tweet = args.Tweet;
                _log.Debug($"Found tweet from {tweet.CreatedBy.ScreenName}");

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
                    _log.Debug(
                        $"Ignoring tweet {tweet.IdStr} as IgnoreRetweets is {stream.IgnoreRetweets} and IsRetweet is {tweet.IsRetweet}");
                    return;
                }

                if (stream.IgnoreReplies && tweet.InReplyToUserId != null)
                {
                    _log.Debug(
                        $"Ignoring tweet {tweet.IdStr} as IgnoreReplies is {stream.IgnoreReplies} and InReplyToUserId is not null (it is {tweet.InReplyToUserId})");
                    return;
                }

                if (tweet.IsRetweet)
                {
                    if (stream.RetweetsToIgnoreByUserId.Contains(tweet.RetweetedTweet.CreatedBy.Id))
                    {
                        _log.Debug($"Ignoring tweet {tweet.IdStr} as author has been ignored");
                        return;
                    }
                }

                foreach (string channel in stream.Channels)
                {
                    if (!_ircClient.JoinedChannels.Contains(channel)) continue;
                    
                    _log.Debug($"Sending tweet from {tweet.CreatedBy.Name} to {channel}");
                    if (_ircClient.IsConnected)
                    {
                        if (tweet.IsRetweet)
                        {
                            _ircClient.SendMessage(SendType.Message, channel,
                                $"{IrcConstants.IrcBold}{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName}):{IrcConstants.IrcNormal} RT @{tweet.RetweetedTweet.CreatedBy.ScreenName} {WebUtility.HtmlDecode(tweet.RetweetedTweet.FullText.ReplaceNonPrintableCharacters(' ').Replace('\n', ' ').Replace('\r', ' '))} - {tweet.Url}");
                            return;
                        }
                        _ircClient.SendMessage(SendType.Message, channel,
                            $"{IrcConstants.IrcBold}{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName}):{IrcConstants.IrcNormal} {WebUtility.HtmlDecode(tweet.FullText.ReplaceNonPrintableCharacters(' ').Replace('\n', ' ').Replace('\r', ' '))} - {tweet.Url}");
                        return;
                    }
                    _log.Debug("Tried to send message to channel but IRC bot is not connected");
                }
            };

            FilteredStream.StreamStopped += (sender, args) =>
            {
                _log.Debug("Twitter stream disconnected with following reason");
                _log.Debug(args.DisconnectMessage?.Reason);
                _backtraceClient?.Send(args.Exception);
                if (args.DisconnectMessage != null) // If socket closed for "reasons" this will be null
                {
                    _log.Debug(
                        $"Twitter disconnect message was: ({args.DisconnectMessage.Code}) {args.DisconnectMessage.Reason}");
                }
                while (true)
                {
                    _log.Debug("Attempting to reconnect to Twitter");
                    FilteredStream.StartMatchingAnyConditionAsync().Wait();
                }
            };

            FilteredStream.WarningFallingBehindDetected += (sender, args) =>
            {
                _log.Debug($"Twitter stream is falling behind. Warning from Twitter: {args.WarningMessage.Message}");
                _log.Debug($"Twitter queue is {args.WarningMessage.PercentFull}% full");
                _backtraceClient?.Send($"Twitter stream falling behind, queue {args.WarningMessage.PercentFull}% full");
            };

            FilteredStream.StartMatchingAnyConditionAsync().Wait();
        }
    }
}