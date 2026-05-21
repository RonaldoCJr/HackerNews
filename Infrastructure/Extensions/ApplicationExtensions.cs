using HackerNews.Application.Interfaces;
using HackerNews.Application.Services;
using HackerNews.Infrastructure.Repositories;

namespace HackerNews.Infrastructure.DependencyInjection
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services)
        {
            services.AddScoped<IStoryService, StoryService>();
            services.AddScoped<IStoryRepository, StoryRepository>();
            services.AddSingleton<ICacheService, MemoryCacheService>();
            return services;
        }
    }
}
