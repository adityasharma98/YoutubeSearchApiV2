using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YoutubeSearchApi.Services
{
    public class FetchVideosFromYoutubeService
    {
        private readonly IMongoCollection<Models.Video> _videos;
        DateTime? _lastSearchQueryTime;
        private int _apiKeyIndex;
        private Dictionary<int, int> _apiKeyToRequestCount = new Dictionary<int, int>();
        private string[] _apiKeys;
        private readonly Timer _timer;

        public FetchVideosFromYoutubeService(IMongoCollection<Models.Video> videos)
        {
            _videos = videos;
            _apiKeyIndex = -1;

            // 3 api keys being used in round-robin fashion to handle quota related 
            // issues
            _apiKeys = new string[]
            {
                "AIzaSyACZBN4qzIUVURWsk1sUAhh0PQOIYgBzz4",
                "AIzaSyCQQxRz8L4EAZxg-eKF8CnayAX_5OJnD84",
                "AIzaSyCUARPxq73NSwlNWIsAqUQ6bEml6Es7FHs",
            };

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(10);

            // spawns a new thread to call GetDataFromYOutube() function
            // at intervals of 10 seconds
            _timer = new Timer(
                async (e) => await GetDataFromYouTube(),
                null,
                startTimeSpan,
                periodTimeSpan);
        }

        public async Task GetDataFromYouTube()
        {
            // each call should be made from a different api in round robin fashion
            _apiKeyIndex = (++_apiKeyIndex) % (_apiKeys.Length);

            //a utility to count number of api requests made per apiKey
            if(_apiKeyToRequestCount.TryGetValue(_apiKeyIndex, out var requestCount))
            {
                _apiKeyToRequestCount[_apiKeyIndex] = ++requestCount;
            }
            else
            {
                _apiKeyToRequestCount[_apiKeyIndex] = 1;
            }
            
            var youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = _apiKeys[_apiKeyIndex],
                    ApplicationName = this.GetType().ToString()
                });

            var searchListRequest = BuildAndGetSearchQuery(youtubeService);

            try
            {
                var response = await searchListRequest.ExecuteAsync();
                Console.WriteLine(response.Items.Count + " videos fetched from youtube");
             
                // if query is successful, find the next set of videos with
                // _lastSearchQueryTime(published time) as the current time
                _lastSearchQueryTime = DateTime.UtcNow;
                var videosList = GetResponseAsVideoList(response);
                AddToStorage(videosList);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Caught exception " + ex.Message);
            }
        }

        private void AddToStorage(List<Models.Video> videosList)
        {
            videosList.ForEach(
                async (video) =>
                {
                    try
                    {
                        await _videos.InsertOneAsync(video);
                    }
                    catch (MongoWriteException mwx)
                    {
                        if (mwx.WriteError.Category == ServerErrorCategory.DuplicateKey)
                        {
                            Console.WriteLine($"Key exists : " + video.Id);
                        }
                    }
                });
        }

        private List<Models.Video> GetResponseAsVideoList(SearchListResponse response)
        {
            return response.Items.Select(_ =>
                new Models.Video
                {
                    Id = _.Id.VideoId,
                    Title = _.Snippet.Title,
                    Description = _.Snippet.Description,
                    PublishTime = _.Snippet.PublishedAt,
                    ThumbNailURLS = _.Snippet.Thumbnails.Default__.Url,
                })
                .ToList();
        }

        private SearchResource.ListRequest BuildAndGetSearchQuery(
            YouTubeService youtubeService)
        {
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = "football";
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;

            searchListRequest.PublishedAfter =
                (_lastSearchQueryTime == null)
                ? _lastSearchQueryTime
                : DateTime.UtcNow.AddMinutes(-5);

            searchListRequest.Type = "video";

            return searchListRequest;
        }
    }
}
