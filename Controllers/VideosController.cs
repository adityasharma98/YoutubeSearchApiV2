using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YoutubeSearchApi.Models;
using YoutubeSearchApi.Services;

namespace YoutubeSearchApi.Controllers
{
    [ApiController]
    [Route("api/{action}")]
    public class VideosController : ControllerBase
    {
        private readonly FetchVideosFromDbService _fetchVideosFromDbService;

        public VideosController(FetchVideosFromDbService fetchVideosFromDbService)
        {
            _fetchVideosFromDbService = fetchVideosFromDbService;
        }

        [HttpGet]
        public async Task<IEnumerable<Video>> GetAllVideos(
            int pageNumber)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var rv = await _fetchVideosFromDbService.GetVideos(
                pageNumber);
            stopWatch.Stop();
            
            Console.WriteLine(
                "Elapsed Time to GetAllVideos : " + stopWatch.ElapsedMilliseconds);
            
            return rv;
        }

        [HttpGet]
        public async Task<IEnumerable<Video>> SearchInVideos(
            string queryString,
            int pageNumber)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var rv = await _fetchVideosFromDbService.SearchVideos(
                queryString,
                pageNumber);
            
            stopWatch.Stop();
            Console.WriteLine(
                "Elapsed Time to SearchInVideos : " + stopWatch.ElapsedMilliseconds);

            return rv;
        }
    }
}
