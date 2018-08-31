using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrumpBot.Models
{
    public class YouTubeApiModels
    {
        public class ContentDetailsRoot
        {
            [JsonProperty("kind")]
            public string Kind { get; set; }
            [JsonProperty("etag")]
            public string Etag { get; set; }
            [JsonProperty("pageInfo")]
            public PageInfoModel PageInfo { get; set; }
            [JsonProperty("items")]
            public List<ItemModel> Items { get; set; }
        }

        public class PageInfoModel
        {
            [JsonProperty("totalResults")]
            public long TotalResults { get; set; }
            [JsonProperty("resultsPerPage")]
            public long ResultsPerPage { get; set; }
        }

        public class SnippetModel
        {
            [JsonProperty("publishedAt")]
            public DateTime PublishedAt { get; set; }
            [JsonProperty("channelId")]
            public string ChannelId { get; set; }
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("channelTitle")]
            public string ChannelTitle { get; set; }
            [JsonProperty("tags")]
            public List<string> Tags { get; set; }
            // Should be an int but it is returned as a string lol
            [JsonProperty("categoryId")]
            public string CategoryId { get; set; }
        }

        public class ContentDetailsModel
        {
            // The formatting on this is retarded, PT4M23S for a 4 minute and 23 second video
            [JsonProperty("duration")]
            public string Duration { get; set; }
            [JsonProperty("dimension")]
            public string Dimension { get; set; }
            [JsonProperty("definition")]
            public string Definition { get; set; }
            [JsonProperty("licensedContent")]
            public bool LicensedContent { get; set; }
            [JsonProperty("projections")]
            public string Projections { get; set; }
        }

        public class StatisticsModel
        {
            [JsonProperty("viewCount")]
            public long ViewCount { get; set; }
            [JsonProperty("likeCount")]
            public long LikeCount { get; set; }
            [JsonProperty("dislikeCount")]
            public long DislikeCount { get; set; }
            // Amazed this is even a thing, it probably doesn't work though
            [JsonProperty("favoriteCount")]
            public long FavoriteCount { get; set; }
            [JsonProperty("commentCount")]
            public long CommentCount { get; set; }
        }

        public class ItemModel
        {
            [JsonProperty("kind")]
            public string Kind { get; set; }
            [JsonProperty("etag")]
            public string Etag { get; set; }
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("snippet")]
            public SnippetModel Snippet { get; set; }
            [JsonProperty("contentDetails")]
            public ContentDetailsModel ContentDetails { get; set; }
            [JsonProperty("statistics")]
            public StatisticsModel Statistics { get; set; }
        }
    }
}