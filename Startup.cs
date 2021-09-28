using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using YoutubeSearchApi.Models;
using YoutubeSearchApi.Services;

namespace YoutubeSearchApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.Configure<VideoStoreDataBaseSettings>(
                Configuration.GetSection(nameof(VideoStoreDataBaseSettings)));

            services.AddSingleton<IVideoStoreDataBaseSettings>(
                sp =>
                    sp.GetRequiredService<IOptions<VideoStoreDataBaseSettings>>().Value);

            services.AddControllers();

            services.AddSingleton(
                sp =>
                {
                    var settings = sp.GetService<IVideoStoreDataBaseSettings>();
                    var client = new MongoClient(settings.ConnectionString);
                    var db = client.GetDatabase(settings.DatabaseName);

                    var videosCollection = db.GetCollection<Models.Video>(settings.CollectionName);

                    var indexKeysDefinition =
                        Builders<Video>.IndexKeys
                            .Ascending(v => v.Id)
                            .Descending(v => v.PublishTime)
                            .Ascending(v => v.Title)
                            .Ascending(v => v.Description);

                    videosCollection.Indexes.CreateOneAsync(
                        new CreateIndexModel<Video>(indexKeysDefinition)).Wait();

                    return videosCollection;
                });

            services.AddSingleton<FetchVideosFromYoutubeService>();

            services.AddSingleton<FetchVideosFromDbService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Initialize(
                serviceProvider);
        }

        private void Initialize(
            IServiceProvider serviceProvider)
        {
            _ = serviceProvider.GetService<FetchVideosFromYoutubeService>();
        }
    }
}
