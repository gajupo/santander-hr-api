using Polly;
using Polly.Extensions.Http;

namespace santander_hr_api.Config
{
    public static class PollyPolicyHelper
    {
        public static IAsyncPolicy<HttpResponseMessage> GetSimpleRetryPolicy()
        {
            // Retries 3 times for transient HTTP errors (5xx or 408)
            return HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3);
        }
    }
}
