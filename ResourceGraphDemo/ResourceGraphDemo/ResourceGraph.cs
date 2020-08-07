using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.Subscription;
using Microsoft.Azure.Management.Subscription.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ResourceGraphDemo
{
    public static class ResourceGraph
    {
        [FunctionName("AzureResourceGraphFunction")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Starting function...");
            var resources = new QueryResponse();

            try
            {
                var credentials = AzureCredential.GetCredentialsFromSp();
                var subscriptionClient = new SubscriptionClient(credentials);
                var resourceGraphClient = new ResourceGraphClient(credentials);
                var subscriptionQuery =
                    Environment.GetEnvironmentVariable("GET_SUBSCRIPTIONS_QUERY", EnvironmentVariableTarget.Process);

                IEnumerable<SubscriptionModel> subscriptions = await subscriptionClient.Subscriptions.ListAsync();
                var subscriptionIds = subscriptions
                    .Where(s => s.State == SubscriptionState.Enabled)
                    .Select(s => s.SubscriptionId)
                    .ToList();

                const int groupSize = 100;
                for (var i = 0; i <= subscriptionIds.Count / groupSize; ++i)
                {
                    var currSubscriptionGroup = subscriptionIds.Skip(i * groupSize).Take(groupSize).ToList();
                    var query = new QueryRequest
                    {
                        Subscriptions = currSubscriptionGroup,
                        Query = subscriptionQuery,
                        Options = new QueryRequestOptions
                        {
                            ResultFormat = ResultFormat.ObjectArray
                        }
                    };

                    resources = await resourceGraphClient.ResourcesAsync(query);
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        resources.Data.ToString(),
                        Encoding.UTF8,
                        "application/json")
                };
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        "{}",
                        Encoding.UTF8,
                        "application/json")
                };
            }
        }
    }
}