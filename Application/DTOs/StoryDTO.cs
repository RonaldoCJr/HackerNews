namespace HackerNews.Application.Models
{
    public record StoryDTO(string Title ,string Uri, string PostedBy, DateTimeOffset Time, int Score, int CommentCount);
}