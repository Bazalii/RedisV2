using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace CommonLibrary.Extensions;

public static class DependencyExtensions
{
    public static void AddDefaultWebClientPolicy(this IServiceCollection serviceCollection)
    {
        var timeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(2));
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(15));

        var defaultHttpClientStrategy = Policy.WrapAsync(
            timeoutPolicy, circuitBreakerPolicy);

        var policyRegistry = serviceCollection.AddPolicyRegistry();

        policyRegistry.Add("default", defaultHttpClientStrategy);
    }
}