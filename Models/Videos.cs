using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace YoutubeSearchApi.Models
{
    public class Video
    {
        [BsonId]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? PublishTime { get; set; }

        public string ThumbNailURLS { get; set; }
    }
}
