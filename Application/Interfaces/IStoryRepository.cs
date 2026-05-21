
using HackerNews.Domain.Entities;

namespace HackerNews.Application.Interfaces
{
    public interface IStoryRepository
    {
        Task<int[]> GetBestStoriesIdsAsync();
        Task<Item?> GetStoryAsync(int id);
    }
}