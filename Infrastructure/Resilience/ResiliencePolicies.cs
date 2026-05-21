using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace HackerNews.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> Timeout(int seconds)
        => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(seconds));

    public static IAsyncPolicy<HttpResponseMessage> Retry(int retries)
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retries, attempt =>
                TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker(int failures, int breakSeconds)
        => HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: failures,
                durationOfBreak: TimeSpan.FromSeconds(breakSeconds)
            );

    public static IAsyncPolicy<HttpResponseMessage> Fallback(string jsonResponse)
        => Policy<HttpResponseMessage>
            .Handle<Exception>()
            .FallbackAsync(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse)
                }
            );
}