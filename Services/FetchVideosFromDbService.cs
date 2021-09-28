using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeSearchApi.Models;

namespace YoutubeSearchApi.Services
{
    public class FetchVideosFromDbService
    {
        private readonly IMongoCollection<Video> _videos;

        public FetchVideosFromDbService(IMongoCollection<Video> videos)
        {
            _videos = videos;
        }

        internal async Task<IEnumerable<Video>> GetVideos(
            int pageNumber)
        {
            var builder = Builders<Video>.Sort;

            var sortBy = builder.Descending(_ => _.PublishTime);

            // taking a page size as 10 videos we skip the videos 
            // already present in previous pages
            return await _videos
                .Find(FilterDefinition<Video>.Empty)
                .Skip((pageNumber - 1) * 10)
                .Limit(10)
                .Sort(sortBy)
                .ToListAsync();
        }

        internal async Task<IEnumerable<Video>> SearchVideos(
            string queryString,
            int pageNumber)
        {
            var builder = Builders<Video>.Filter;
            FilterDefinition<Video> filter = FilterDefinition<Video>.Empty;

            var words = queryString.Split();

            foreach(var word in words)
            {
                //using regEx for partial match of words 

                var queryExpr =
                    new BsonRegularExpression(
                        new Regex(
                            word,
                            RegexOptions.IgnoreCase));

                filter |= builder.Regex("Title", queryExpr);
                filter |= builder.Regex("Description", queryExpr);
            }


            // taking a page size as 10 videos we skip the videos 
            // already present in previous pages
            return (
                await _videos
                    .Find(filter)
                    .Skip((pageNumber-1)*10)
                    .Limit(10)
                    .ToListAsync())
                .OrderByDescending(
                    _ => _.PublishTime);
        }
    }
}
