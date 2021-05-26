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

namespace HealthGateway.WebClient.Delegates
{

    /// <summary>
    /// Implementation that uses HTTP to create/revoke Connections and Credentials
    /// </summary>
    public class WalletIssuerDelegate : IWalletIssuerDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly HttpClient client;

        private static readonly string SchemaName = "vaccine";
        private static readonly string SchemaVersion = "1.0";

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletIssuerDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        public WalletIssuerDelegate(
            ILogger<WalletIssuerDelegate> logger,
            IHttpClientService httpClientService)

        {
            this.logger = logger;
            this.httpClientService = httpClientService;

            //TODO NEED TO STORE IN OPENSHIFT + Constants file?
            string bearerToken = "acapy agent api key";

            this.client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        }

        /// <inheritdoc/>
        public async Task<CreateConnectionResponse> CreateConnectionAsync(string walletConnectionId)
        {
            return await CreateInvitationAsync(walletConnectionId);
        }

        /// <inheritdoc/>
        public bool RevokeConnectionAsync(string agentId)
        {
            return false;
        }

        /// <inheritdoc/>
        public async Task<string> IssueCredentialAsync(string agentId, ImmunizationCredential immunizationCredential)
        {
            var issuerDid = await GetIssuerDidAsync();
            var schemaId = await GetSchemaId(issuerDid);
            if(schemaId == null)
            {
                schemaId =  await CreateSchemaAsync();
            }

            var credentialDefinitionId = await GetCredentialDefinitionIdAsync(schemaId);
            if(credentialDefinitionId == null)
            {
                credentialDefinitionId = await CreateCredentialDefinitionAsync(schemaId);
            }

            var credentialAttributes = CreateCredentialAttributesAsync(immunizationCredential);
            var credentialOffer = await CreateCredentialOfferAsync(agentId, credentialAttributes);
            var issueCredentialResponse = await IssueCredentialSendAsync(credentialOffer);

            return (string)issueCredentialResponse.SelectToken("credential_exchange_id");
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeCredentialAsync(string credentialExchangeId, string agentId, DateTime? addedDateTime)
        {
            // If addedDateTime is set, revoke credential
            // If addedDateTime is null then credential was never accepted, delete credential
            var success = addedDateTime == null
                ? await DeleteCredentialAsync(credentialExchangeId)
                : await RevokeIssuedCredentialAsync(credentialExchangeId);

            if (success)
            {
                await SendMessageAsync(agentId, "This credential has been revoked.");
            }

            return success;
        }

        // Create the credential offer.
        private async Task<JObject> CreateCredentialOfferAsync(string connectionId, JArray attributes)
        {
            var issuerDid = await GetIssuerDidAsync();
            var schemaId = await GetSchemaId(issuerDid);
            var schema = (await GetSchema(schemaId)).Value<JObject>("schema");
            var credentialDefinitionId = await GetCredentialDefinitionIdAsync(schemaId);

            JObject credentialOffer = new JObject
            {
                { "connection_id", connectionId },
                { "issuer_did", issuerDid },
                { "schema_id", schemaId },
                { "schema_issuer_did", issuerDid },
                { "schema_name", schema.Value<string>("name") },
                { "schema_version", schema.Value<string>("version") },
                { "cred_def_id", credentialDefinitionId },
                { "comment", "PharmaNet GPID" },
                { "auto_remove", false },
                { "trace", false },
                {
                    "credential_proposal",
                    new JObject
                        {
                            { "@type", "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/issue-credential/1.0/credential-preview" },
                            { "attributes", attributes }
                        }
                }
            };

            logger.LogInformation("Credential offer for connection ID \"{connectionId}\" for {@JObject}", connectionId, JsonConvert.SerializeObject(credentialOffer));

            return credentialOffer;
        }

        // Create the credential proposal attributes.
        private JArray CreateCredentialAttributesAsync(ImmunizationCredential immunizationCredential)
        {
            var attributes = new JArray
            {
                new JObject
                {
                    { "name", "RecipientName" },
                    { "value", immunizationCredential.RecipientName }
                },
                new JObject
                {
                    { "name", "RecipientBirthDate" },
                    { "value", immunizationCredential.RecipientBirthDate }
                },
                new JObject
                {
                    { "name", "RecipientPHN" },
                    { "value", immunizationCredential.RecipientPHN }
                },
                new JObject
                {
                    { "name", "ImmunizationType" },
                    { "value", immunizationCredential.ImmunizationType }
                },
                new JObject
                {
                    { "name", "ImmunizationProduct" },
                    { "value", immunizationCredential.ImmunizationProduct }
                },
                new JObject
                {
                    { "name", "ImmunizationAgent" },
                    { "value", immunizationCredential.ImmunizationAgent }
                },
                new JObject
                {
                    { "name", "LotNumber" },
                    { "value", immunizationCredential.LotNumber }
                },
                new JObject
                {
                    { "name", "Provider" },
                    { "value", immunizationCredential.Provider }
                }
            };

            logger.LogInformation("Credential offer attributes for {@JObject}", JsonConvert.SerializeObject(attributes));

            return attributes;
        }

        private async Task<CreateConnectionResponse> CreateInvitationAsync(string alias)
        {
            logger.LogInformation("Create connection invitation");

            List<KeyValuePair<string?, string?>> values = new();
            var httpContent = new FormUrlEncodedContent(values);

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"connections/create-invitation?alias={alias}", httpContent);
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

        private async Task<JObject> IssueCredentialSendAsync(JObject credentialOffer)
        {
            var httpContent = new StringContent(credentialOffer.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync("issue-credential/send", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to issue a credential: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::IssueCredentialAsync");
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        private async Task<bool> RevokeIssuedCredentialAsync(string credentialExchangeId)
        {
            logger.LogInformation("Revoking credential cred_ex-Id={id}", credentialExchangeId);

            var revocationObject = new
            {
                cred_ex_id = credentialExchangeId,
                publish = true
            };

            var httpContent = new StringContent(revocationObject.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"revocation/revoke", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to revoke a credential: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::RevokeCredentialAsync");
            }

            logger.LogInformation("Revoke credential cred_ex_id={id} success", credentialExchangeId);

            return true;
        }

        private async Task<string?> GetSchemaId(string did)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await client.GetAsync($"schemas/created?schema_version={SchemaVersion}&schema_issuer_did={did}&schema_name={SchemaName}");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to get the schema id by issuer did: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::GetSchema");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());
            var schemas = (JArray)body.SelectToken("schema_ids");

            if(schemas != null && schemas.Count > 0)
            {
                logger.LogInformation("SCHEMA_ID: {schemaid}", (string)body.SelectToken("schema_ids[0]"));
                return (string)body.SelectToken("schema_ids[0]");
            }
            else {
                return null;
            }
        }

        private async Task<JObject> GetSchema(string schemaId)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await client.GetAsync($"schemas/{schemaId}");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to get the schema: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::GetSchema");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

            logger.LogInformation("GET Schema response {@JObject}", JsonConvert.SerializeObject(body));

            return body;
        }

        private async Task<string> CreateSchemaAsync()
        {
            JObject propertiesObject = JObject.FromObject(new ImmunizationCredential{});

            var attributes = new JArray{};
            foreach(var property in propertiesObject.Properties())
            {
                attributes.Add(property.Name);
            }

            var schema = new JObject
            {
                { "attributes", attributes },
                { "schema_name", SchemaName },
                { "schema_version", SchemaVersion}
            };

            var httpContent = new StringContent(schema.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"schemas", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to create a schema: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::CreateSchemaAsync");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

            logger.LogInformation("Schema Created successfully {@JObject}", JsonConvert.SerializeObject(body));

            return (string)body.SelectToken("schema_id");
        }

        private async Task<string> GetIssuerDidAsync()
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await client.GetAsync("wallet/did/public");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to get the issuer DID: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::GetIssuerDidAsync");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

            logger.LogInformation("GET Issuer DID response {did}", (string)body.SelectToken("result.did"));

            return (string)body.SelectToken("result.did");
        }

        private async Task<string?> GetCredentialDefinitionIdAsync(string schemaId)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await client.GetAsync($"credential-definitions/created?schema_id={schemaId}");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to get credential definition: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::GetCredentialDefinitionAsync");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray credentialDefinitionIds = (JArray)body.SelectToken("credential_definition_ids");

            logger.LogInformation("GET Credential Definition IDs {@JObject}", JsonConvert.SerializeObject(body));
            if(credentialDefinitionIds != null && credentialDefinitionIds.Count > 0)
            {
                return (string)body.SelectToken($"credential_definition_ids[{credentialDefinitionIds.Count - 1}]");
            }

            return null;
        }

        private async Task<string> CreateCredentialDefinitionAsync(string schemaId)
        {
            var credentialDefinition = new JObject
            {
                { "schema_id", schemaId },
                { "support_revocation", true },
                { "tag", "test" }
            };

            var httpContent = new StringContent(credentialDefinition.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"credential-definitions", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to create a credential-definition: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::CreateCredentialDefinitionAsync");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

            logger.LogInformation("Credential Definition Created successfully {@JObject}", JsonConvert.SerializeObject(body));

            return (string)body.SelectToken("credential_definition_id");
        }

        private async Task<JObject> GetPresentationProof(string presentationExchangeId)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await client.GetAsync($"presentation-proof/records/{presentationExchangeId}");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to get presentation proof: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                throw new AcaPyApiException($"Error code {response.StatusCode} was provided when calling VerifiableCredentialClient::GetPresentationProof");
            }

            JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

            logger.LogInformation("GET Presentation proof @JObject", JsonConvert.SerializeObject(body));

            return body;
        }

        private async Task<bool> DeleteCredentialAsync(string credentialExchangeId)
        {
            logger.LogInformation("Deleting credential cred_ex-Id={id}", credentialExchangeId);

            HttpResponseMessage? response = null;
            try
            {
                response = await client.DeleteAsync($"issue-credential/records/{credentialExchangeId}");
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to delete a credential: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                return false;
            }

            logger.LogInformation("Deleting credential cred_ex_id={id} success", credentialExchangeId);

            return true;
        }

        private async Task<bool> SendMessageAsync(string connectionId, string content)
        {
            logger.LogInformation("Sending a message to connection_id={id}", connectionId);

            var messageObject = new { content };

            var httpContent = new StringContent(messageObject.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await client.PostAsync($"connections/{connectionId}/send-message", httpContent);
            }
            catch (Exception ex)
            {
                await LogError(response, ex);
                throw new AcaPyApiException("Error occurred attempting to send a message to the connection: ", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                await LogError(response);
                return false;
            }

            return true;
        }

        private async Task LogError(HttpResponseMessage response, Exception? exception = null)
        {
            await LogError(null, response, exception);
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
            }
            else
            {
                secondaryMessage = "No additional message. Http response and exception were null.";
            }

            logger.LogError(exception, secondaryMessage, new Object[] { content, response });
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
