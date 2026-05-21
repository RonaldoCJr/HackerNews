using HackerNews.Domain.Entities;
using Refit;

namespace HackerNews.Infrastructure.Interfaces
{
    public interface IHackerNewsApi
    {
        [Get("/beststories.json")]
        Task<ApiResponse<int[]>> GetBestStoriesIdsAsync();

        [Get("/item/{itemId}.json")]
        Task<ApiResponse<Story>> GetStoryAsync(int itemId);
    }
}
