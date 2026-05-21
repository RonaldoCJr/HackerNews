namespace HackerNews.Infrastructure.Resilience;

public class FailureSimulationHandler : DelegatingHandler
{
    //Only for testing
    public string Scenario { get; set; } = "SUCCESS";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (Scenario == "TIMEOUT")
        {
            await Task.Delay(4000, cancellationToken);
        }

        if (Scenario == "RETRY_THEN_SUCCESS")
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request, 
                Content = new StringContent(string.Empty)
            };
            return response;
        }

        if (Scenario == "CIRCUIT_BREAKER")
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                RequestMessage = request, 
                Content = new StringContent(string.Empty)
            };
            return response;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}