using HackerNews.Application.Interfaces;
using HackerNews.Infrastructure.Interfaces;
using HackerNews.Infrastructure.Resilience;
using HackerNews.Infrastructure.Settings;
using Refit;

namespace HackerNews.Infrastructure.DependencyInjection
{
    public static class RefitExtensions
    {
        public static IServiceCollection AddRefitSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var hnApiSettings = configuration.GetSection("HackerNewsApi").Get<HackerNewsApiSettings>() ?? throw new InvalidOperationException("HackerNewsApi settings are missing.");

            var simulationHandler = new FailureSimulationHandler { Scenario = "RETRY_THEN_SUCCESS" };
            services.AddSingleton(simulationHandler);

            services.AddRefitClient<IHackerNewsApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri($"{hnApiSettings.BaseUrl.TrimEnd('/')}/{hnApiSettings.Version}/");
                    c.DefaultRequestHeaders.Add("User-Agent", "HackerNews-API");
                })
                .AddPolicyHandlerFromRegistry("external-api-policy")
                .AddHttpMessageHandler(() => simulationHandler);


            return services;
        }
    }
}
