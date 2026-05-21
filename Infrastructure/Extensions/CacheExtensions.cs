using HackerNews.Infrastructure.Settings;

namespace HackerNews.Infrastructure.DependencyInjection
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCustomCacheSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("CacheSettings");
            services.Configure<CacheSettings>(section);

            var cacheSettings = section.Get<CacheSettings>();

            if (cacheSettings != null && cacheSettings.DefaultExpirationMinutes > 0)
            {
                services.AddOutputCache(options =>
                {
                    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(cacheSettings.DefaultExpirationMinutes)));
                });
            }

            return services;
        }
    }
}
