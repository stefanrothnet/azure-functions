using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SetSubscriptionTags
{
    public static class SubscriptionTag
    {
        [FunctionName("SubscriptionTag")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var request = await DeserializeRequest(req);

            log.LogWarning(
                $"Attempting to tag subscription ID: {request.Id}");

            var result =
                await SetSubscriptionTags(request.Tags, request.Id,
                    AzureCredential.GetCredentialsFromSp());
            return new OkObjectResult($"Setting subscription tags was successfull: {result.IsSuccessStatusCode}");
        }

        private static async Task<HttpResponseMessage> SetSubscriptionTags(Dictionary<string, string> subscriptionTags,
            string subscriptionId, AzureCredentials credentials)
        {
            var subscriptionTagEndpoint =
                string.Format(
                    Environment.GetEnvironmentVariable("ARM_TAG_SUBSCRIPTION", EnvironmentVariableTarget.Process),
                    subscriptionId);
            var cancellationToken = new CancellationToken();

            var client = RestClient
                .Configure()
                .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
                .WithCredentials(credentials)
                .Build();

            var json = JsonConvert.SerializeObject(new
                {
                    properties = new
                    {
                        tags = subscriptionTags
                    }
                }
            );

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(subscriptionTagEndpoint),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            client.Credentials.ProcessHttpRequestAsync(requestMessage, cancellationToken).GetAwaiter().GetResult();
            var httpClient = new HttpClient();
            var response = await httpClient
                .SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            //https://github.com/Azure/azure-libraries-for-net/issues/949
            return response;
        }

        private static async Task<Subscription> DeserializeRequest(HttpRequest req)
        {
            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var subscription = JsonConvert.DeserializeObject<Subscription>(content);
            return subscription;
        }
    }
}