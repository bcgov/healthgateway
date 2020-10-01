
namespace Keycloak.Authorization.Client
{
    using System;
    using System.IO;
    using HttpClient;
    using Microsoft.Extensions.Configuration;

    public class AuthZClient
    {
        private readonly HttpClient httpClient;
        private readonly TokenCallable patSupplier;

        public AuthZClient(IConfiguration configuration, ILogger<AuthZClient> logger)
        {

        }
    }
}
