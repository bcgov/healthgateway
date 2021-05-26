using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using HealthGateway.Common.Services;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using HealthGateway.WebClient.Server.Models.AcaPy;
using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using HealthGateway.WebClient.Models;

namespace HealthGateway.WebClient.Delegates
{

    /// <summary>
    /// Implementation that uses HTTP to create/revoke Connections and Credentials
    /// </summary>
    public class WalletIssuerDelegate : IWalletIssuerDelegate
    {
        private const string AcapyConfigSectionKey = "AcaPy";
        private static readonly string SchemaName = "vaccine";
        private static readonly string SchemaVersion = "1.0";

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly HttpClient client;
        private readonly WalletIssuerConfiguration walletIssuerConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletIssuerDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public WalletIssuerDelegate(
            ILogger<WalletIssuerDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)

        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.walletIssuerConfig = new WalletIssuerConfiguration();
            configuration.Bind(AcapyConfigSectionKey, this.walletIssuerConfig);

            string bearerToken = this.walletIssuerConfig.AgentApiKey;

            this.client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.walletIssuerConfig.AgentApiUrl);
        }

        /// <inheritdoc/>
        public async Task<CreateConnectionResponse> CreateConnectionAsync(string walletConnectionId)
        {
             logger.LogInformation("Create connection invitation");

            List<KeyValuePair<string?, string?>> values = new();
            var httpContent = new FormUrlEncodedContent(values);

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"connections/create-invitation?alias={walletConnectionId}", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(httpContent, response, ex);
                throw new AcaPyApiException("Error occurred when calling AcaPy API. Try again later.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(httpContent, response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling WalletIssuerDelegate::CreateInvitationAsync");
            }

            CreateConnectionResponse createConnectionResponse = await response.Content.ReadAsAsync<CreateConnectionResponse>();

            logger.LogInformation("Create connection invitation response {@JObject}", JsonSerializer.Serialize(createConnectionResponse));

            return createConnectionResponse;
        }

        private async Task LogError(HttpContent content, HttpResponseMessage response, Exception? exception = null)
        {
            string secondaryMessage;
            if (exception != null)
            {
                secondaryMessage = $"Exception: {exception.Message}";
            }
            else if (response != null)
            {
                string responseMessage = await response.Content.ReadAsStringAsync();
                secondaryMessage = $"Response code: {(int)response.StatusCode}, response body:{responseMessage}";
                logger.LogError(exception, secondaryMessage, new Object[] { content, response });
            }
            else
            {
                secondaryMessage = "No additional message. Http response and exception were null.";
                logger.LogError(exception, secondaryMessage, new Object[] { content });
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
    /// </summary>
    public class AcaPyApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        public AcaPyApiException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public AcaPyApiException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcaPyApiException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public AcaPyApiException(string message, Exception inner) : base(message, inner) { }
    }

}
