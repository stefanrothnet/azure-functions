using System;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace SetSubscriptionTags
{
    internal static class AzureCredential
    {
        internal static string TenantId =
            Environment.GetEnvironmentVariable("TENANTID", EnvironmentVariableTarget.Process);

        internal static string ClientId =
            Environment.GetEnvironmentVariable("SP_CLIENTID", EnvironmentVariableTarget.Process);

        internal static string ClientSecret =
            Environment.GetEnvironmentVariable("SP_KEY", EnvironmentVariableTarget.Process);

        public static AzureCredentials GetCredentialsFromSp()
        {
            var credentials = new AzureCredentialsFactory().FromServicePrincipal(ClientId, ClientSecret, TenantId,
                AzureEnvironment.AzureGlobalCloud);
            return credentials;
        }
    }
}