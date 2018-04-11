using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Meebey.SmartIrc4net;

namespace TrumpBot
{
    internal class MessageInterval
    {
        public class Message
        {
            public string Channel { get; set; }
            public List<string> Messages { get; set; }
            public int SecondsInterval { get; set; }
            public bool Active { get; set; }
        }

        internal class RunningThread
        {
            internal string Channel { get; set; }
            internal Thread Thread { get; set; }
            internal Message Message { get; set; }
        }

        private IrcClient _ircClient;
        private List<RunningThread> _runningThreads = new List<RunningThread>();
        private List<Message> _messages;

        internal MessageInterval(List<Message> messages, IrcClient ircClient)
        {
            _ircClient = ircClient;
            _messages = messages;

            InitialiseMessaages();
        }

        internal void CreateThread(Message message)
        {
            Thread newThread = new Thread(() => MessageThread(message));
            _runningThreads.Add(new RunningThread
            {
                Channel = message.Channel,
                Thread = newThread,
                Message = message
            });
            newThread.Start();
        }

        internal void MessageThread(Message message)
        {
            while (message.Active)
            {
                Thread.Sleep(message.SecondsInterval * 1000); // Thread.Sleep static method takes milliseconds
                foreach (string line in message.Messages)
                {
                    _ircClient.SendMessage(SendType.Message, message.Channel, line);
                }
            }
        }

        internal void StopThread(string channel)
        {
            RunningThread runningThread =
                (from thread in _runningThreads where thread.Channel == channel select thread).FirstOrDefault();

            if (runningThread == null) return;

            runningThread.Thread.Abort();
            _runningThreads.Remove(runningThread);
        }

        internal void RehashMessages(List<Message> newMessages)
        {
            foreach (Message message in _messages)
            {
                StopThread(message.Channel);
            }

            _messages = newMessages;
            InitialiseMessaages();
        }

        internal void InitialiseMessaages()
        {
            foreach (Message message in _messages)
            {
                if (!message.Active && _ircClient.JoinedChannels.Contains(message.Channel)) continue;

                CreateThread(message);
            }
        }
    }
}
