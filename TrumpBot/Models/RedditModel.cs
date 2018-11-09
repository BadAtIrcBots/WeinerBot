using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class RedditModel
    {
        public class SubredditModel
        {
            public class Thread
            {
                [JsonProperty("domain")]
                public string Domain { get; set; }

                [JsonProperty("banned_by")]
                public string BannedBy { get; set; } = "N/A";

                [JsonProperty("subreddit")]
                public string SubReddit { get; set; }

                [JsonProperty("selftext_html")]
                public string SelfTextHtml { get; set; }

                [JsonProperty("selftext")]
                public string SelfText { get; set; }

                [JsonProperty("likes")]
                public int? Likes { get; set; }

                [JsonProperty("id")]
                public string Id { get; set; }

                [JsonProperty("gilded")]
                public int GildedCount { get; set; }

                [JsonProperty("author")]
                public string Author { get; set; }

                [JsonProperty("score")]
                public int? Score { get; set; }

                [JsonProperty("over_18")]
                public bool IsNsfw { get; set; }

                [JsonProperty("num_comments")]
                public int CommentCount { get; set; }

                [JsonProperty("subreddit_id")]
                public string SubRedditId { get; set; }

                [JsonProperty("post_hint")]
                public string PostHint { get; set; }

                [JsonProperty("is_self")]
                public bool IsSelf { get; set; }

                [JsonProperty("stickied")]
                public bool IsSticky { get; set; }

                [JsonProperty("permalink")]
                public string Permalink { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }
                
                [JsonProperty("created_utc")]
                public long CreatedUtc { get; set; }

                [JsonProperty("url")]
                public Uri Url { get; set; }

                [JsonProperty("author_flair_text")]
                public string AuthorFlairText { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }
                
                [JsonProperty("is_video")]
                public bool IsVideo { get; set; }
            }

            public class SubredditChildren
            {
                [JsonProperty("kind")]
                public string Kind { get; set; }

                [JsonProperty("data")]
                public Thread Thread { get; set; }
            }

            public class SubredditData
            {
                [JsonProperty("modhash")]
                public string ModHash { get; set; }

                [JsonProperty("children")]
                public List<SubredditChildren> Children { get; set; }
            }

            public class Subreddit
            {
                [JsonProperty("kind")]
                public string Kind { get; set; }

                [JsonProperty("data")]
                public SubredditData Data { get; set; }
            }
        }
    }
}
