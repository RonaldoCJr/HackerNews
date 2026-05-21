using HackerNews.Application.Models;
using HackerNews.Domain.Entities;

namespace HackerNews.Application.Mappers
{
    public static class StoryMapper
    {
        public static StoryDTO ToDTO(this Item item)
        {
            return new StoryDTO(
                Title: item.Title ?? string.Empty,
                Uri: item.Url ?? string.Empty,
                PostedBy: item.By,
                Time: DateTimeOffset.FromUnixTimeSeconds(item.Time),
                Score: item.Score,
                CommentCount: item.Descendants
            );
        }
    }
}
