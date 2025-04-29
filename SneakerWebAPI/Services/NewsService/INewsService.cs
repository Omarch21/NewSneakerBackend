namespace SneakerWebAPI.Services.NewsService
{
    public interface INewsService
    {
        Task<List<Newsfeed>> GetNews();
    }
}
