using HackerNews.Infrastructure.Resilience;
using Polly;

namespace HackerNews.Infrastructure.Extensions
{
    public static class PolicyRegistryExtensions
    {
        public static void AddResiliencePolicies(this IServiceCollection services)
        {
            var registry = services.AddPolicyRegistry();

            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            registry.Add(
                "external-api-policy",
                Policy.WrapAsync(
                    ResiliencePolicies.CircuitBreaker(failures: 5, breakSeconds: 15),
                    retryPolicy,
                    ResiliencePolicies.Timeout(2)));
        }
    }
}
