using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;
using HackerNews.Infrastructure.Exceptions;
using HackerNews.Infrastructure.Interfaces;

namespace HackerNews.Infrastructure.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly IHackerNewsApi _api;

        public StoryRepository(IHackerNewsApi api)
        {
            _api = api;
        }

        public async Task<int[]> GetBestStoriesIdsAsync()
        {
            var response = await _api.GetBestStoriesIdsAsync();

            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException("Error fetching story ids", (int) response.StatusCode);

            return response.Content!;
        }

        public async Task<Item?> GetStoryAsync(int id)
        {
            var response = await _api.GetStoryAsync(id);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException($"Error fetching story {id}", (int) response.StatusCode);

            return response.Content;
        }
    }
}
