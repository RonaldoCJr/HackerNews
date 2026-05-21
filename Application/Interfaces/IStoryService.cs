using HackerNews.Application.Models;

namespace HackerNews.Application.Interfaces
{
    public interface IStoryService
    {
        Task<IEnumerable<StoryDTO>> GetBestsAsync(int n);
    }
}
