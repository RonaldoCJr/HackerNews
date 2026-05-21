using HackerNews.Application.Interfaces;
using HackerNews.Application.Mappers;
using HackerNews.Application.Models;

namespace HackerNews.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly ICacheService _cacheService;

        public StoryService(IStoryRepository storyRepository, ICacheService cacheService)
        {
            _storyRepository = storyRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<StoryDTO>> GetBestsAsync(int n)
        {
            var storiesIdResult = await _cacheService.GetOrCreateAsync("best_story_ids", 
                () => _storyRepository.GetBestStoriesIdsAsync(), TimeSpan.FromSeconds(20));

            var tasks = storiesIdResult!.Select(async id =>
            {
                return await _cacheService.GetOrCreateAsync($"story_{id}", async () =>
                {
                    return await _storyRepository.GetStoryAsync(id);
                });
            });

            var allItems = await Task.WhenAll(tasks);

            return allItems
                .Where(s => s != null)
                .OrderByDescending(s => s!.Score)
                .Take(n)
                .Select(s=> s.ToDTO());
        }
    }
}
