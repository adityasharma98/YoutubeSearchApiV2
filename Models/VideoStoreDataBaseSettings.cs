namespace YoutubeSearchApi.Models
{
    public class VideoStoreDataBaseSettings : IVideoStoreDataBaseSettings
    {
        public string CollectionName { get; set; }

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

    }

    public interface IVideoStoreDataBaseSettings
    {
        string CollectionName { get; set; }

        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }
}
