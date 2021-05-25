using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AutoMapper;

namespace HealthGateway.WebClient.Delegates
{
    public class WalletIssuerDelegate : BaseService, IWalletIssuerDelegate
    {
        private readonly IMapper _mapper;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        private static readonly string SchemaName = "vaccine";
        private static readonly string SchemaVersion = "1.0";

        public WalletIssuerDelegate(
            ApiDbContext context,
            IHttpContextAccessor httpContext,
            IMapper mapper,
            ILogger<WalletIssuerDelegate> logger)
            : base(context, httpContext)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<WalletConnectionResponse> CreateConnectionAsync(string walletConnectionId)
        {
            var invitation = await CreateInvitationAsync(walletConnectionId);
            var invitationUrl = invitation.Value<string>("invitation_url");

            return invitationUrl;
        }

        public async Task<string> IssueCredentialAsync(string agentId, string attributes)
        {
            var issuerDid = await _verifiableCredentialClient.GetIssuerDidAsync();
            var schemaId = await _verifiableCredentialClient.GetSchemaId(issuerDid);
            if(schemaId == null)
            {
                schemaId =  await _verifiableCredentialClient.CreateSchemaAsync();
            }

            var credentialDefinitionId = await _verifiableCredentialClient.GetCredentialDefinitionIdAsync(schemaId);
            if(credentialDefinitionId == null)
            {
                credentialDefinitionId = await _verifiableCredentialClient.CreateCredentialDefinitionAsync(schemaId);
            }

            var credentialAttributes = await CreateCredentialAttributesAsync(attributes);
            var credentialOffer = await CreateCredentialOfferAsync(agentId, credentialAttributes);
            var issueCredentialResponse = await _verifiableCredentialClient.IssueCredentialAsync(credentialOffer);

            return (string)issueCredentialResponse.SelectToken("credential_exchange_id");
        }


        public async Task<bool> RevokeCredentialAsync(string credentialExchangeId, string agentId, DateTime? addedDateTime)
        {
            // If addedDateTime is set, revoke credential
            // If addedDateTime is null then credential was never accepted, delete credential
            var success = addedDateTime == null
                ? await _verifiableCredentialClient.DeleteCredentialAsync(credential)
                : await _verifiableCredentialClient.RevokeCredentialAsync(credential);

            if (success)
            {
                await _verifiableCredentialClient.SendMessageAsync(connectionId, "This credential has been revoked.");
            }

            return success;
        }


        // Create the credential offer.
        private async Task<JObject> CreateCredentialOfferAsync(string connectionId, JArray attributes)
        {
            var issuerDid = await _verifiableCredentialClient.GetIssuerDidAsync();
            var schemaId = await _verifiableCredentialClient.GetSchemaId(issuerDid);
            var schema = (await _verifiableCredentialClient.GetSchema(schemaId)).Value<JObject>("schema");
            var credentialDefinitionId = await _verifiableCredentialClient.GetCredentialDefinitionIdAsync(schemaId);

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

            _logger.LogInformation("Credential offer for connection ID \"{connectionId}\" for {@JObject}", connectionId, JsonConvert.SerializeObject(credentialOffer));

            return credentialOffer;
        }

        // Create the credential proposal attributes.
        private async Task<JArray> CreateCredentialAttributesAsync(int patientId, Guid guid)
        {
            var record = await _immunizationClient.GetImmunizationRecordAsync(guid);

            var immunizationRecord = _mapper.Map<ImmunizationResponse, Schema>(record);

            var attributes = new JArray
            {
                new JObject
                {
                    { "name", "name"},
                    { "value", immunizationRecord.name }
                },
                new JObject
                {
                    { "name", "description"},
                    { "value", immunizationRecord.description }
                },
                new JObject
                {
                    { "name", "expirationDate"},
                    { "value", DateTime.Now.AddYears(1) }
                },
                new JObject
                {
                    { "name", "credential_type" },
                    { "value", immunizationRecord.credential_type }
                },
                new JObject
                {
                    { "name", "countryOfVaccination" },
                    { "value", immunizationRecord.countryOfVaccination }
                },
                new JObject
                {
                    { "name", "recipient_type" },
                    { "value", immunizationRecord.recipient_type }
                },
                new JObject
                {
                    { "name", "recipient_fullName" },
                    { "value", immunizationRecord.recipient_fullName }
                },
                new JObject
                {
                    { "name", "recipient_birthDate" },
                    { "value", immunizationRecord.recipient_birthDate }
                },
                new JObject
                {
                    { "name", "vaccine_type" },
                    { "value", immunizationRecord.vaccine_type }
                },
                new JObject
                {
                    { "name", "vaccine_disease" },
                    { "value", immunizationRecord.vaccine_disease }
                },
                new JObject
                {
                    { "name", "vaccine_medicinalProductName" },
                    { "value", immunizationRecord.vaccine_medicinalProductName }
                },
                new JObject
                {
                    { "name", "vaccine_marketingAuthorizationHolder" },
                    { "value", immunizationRecord.vaccine_marketingAuthorizationHolder }
                },
                new JObject
                {
                    { "name", "vaccine_dateOfVaccination"},
                    { "value", immunizationRecord.vaccine_dateOfVaccination }
                }
            };

            _logger.LogInformation("Credential offer attributes for {@JObject}", JsonConvert.SerializeObject(attributes));

            return attributes;
        }

        private async Task<JObject> CreateInvitationAsync(string alias)
        {
            _logger.LogInformation("Create connection invitation");

            var values = new List<KeyValuePair<string, string>>();
            var httpContent = new FormUrlEncodedContent(values);

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync($"connections/create-invitation?alias={alias}", httpContent);
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

            _logger.LogInformation("Create connection invitation response {@JObject}", JsonConvert.SerializeObject(response));

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        private async Task<JObject> IssueCredentialAsync(JObject credentialOffer)
        {
            var httpContent = new StringContent(credentialOffer.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync("issue-credential/send", httpContent);
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

        private async Task<bool> RevokeCredentialAsync(Credential credential)
        {
            _logger.LogInformation("Revoking credential cred_ex-Id={id}", credential.CredentialExchangeId);

            var revocationObject = new
            {
                cred_ex_id = credential.CredentialExchangeId,
                publish = true
            };

            var httpContent = new StringContent(revocationObject.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync($"revocation/revoke", httpContent);
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

            _logger.LogInformation("Revoke credential cred_ex_id={id} success", credential.CredentialExchangeId);

            return true;
        }

        private async Task<string> GetSchemaId(string did)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync($"schemas/created?schema_version={SchemaVersion}&schema_issuer_did={did}&schema_name={SchemaName}");
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
                _logger.LogInformation("SCHEMA_ID: {schemaid}", (string)body.SelectToken("schema_ids[0]"));
                return (string)body.SelectToken("schema_ids[0]");
            }
            else {
                return null;
            }
        }

        private async Task<JObject> GetSchema(string schemaId)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync($"schemas/{schemaId}");
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

            _logger.LogInformation("GET Schema response {@JObject}", JsonConvert.SerializeObject(body));

            return body;
        }

        private async Task<string> CreateSchemaAsync()
        {
            JObject propertiesObject = JObject.FromObject(new Schema{});

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

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync($"schemas", httpContent);
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

            _logger.LogInformation("Schema Created successfully {@JObject}", JsonConvert.SerializeObject(body));

            return (string)body.SelectToken("schema_id");
        }

        private async Task<string> GetIssuerDidAsync()
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync("wallet/did/public");
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

            _logger.LogInformation("GET Issuer DID response {did}", (string)body.SelectToken("result.did"));

            return (string)body.SelectToken("result.did");
        }

        private async Task<string> GetCredentialDefinitionIdAsync(string schemaId)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync($"credential-definitions/created?schema_id={schemaId}");
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

            _logger.LogInformation("GET Credential Definition IDs {@JObject}", JsonConvert.SerializeObject(body));
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

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync($"credential-definitions", httpContent);
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

            _logger.LogInformation("Credential Definition Created successfully {@JObject}", JsonConvert.SerializeObject(body));

            return (string)body.SelectToken("credential_definition_id");
        }

        private async Task<JObject> GetPresentationProof(string presentationExchangeId)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _client.GetAsync($"presentation-proof/records/{presentationExchangeId}");
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

            _logger.LogInformation("GET Presentation proof @JObject", JsonConvert.SerializeObject(body));

            return body;
        }

        private async Task<bool> DeleteCredentialAsync(string credentialExchangeId)
        {
            _logger.LogInformation("Deleting credential cred_ex-Id={id}", credentialExchangeId);

            HttpResponseMessage response = null;
            try
            {
                response = await _client.DeleteAsync($"issue-credential/records/{credential.CredentialExchangeId}");
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

            _logger.LogInformation("Deleting credential cred_ex_id={id} success", credential.CredentialExchangeId);

            return true;
        }

        private async Task<bool> SendMessageAsync(string connectionId, string content)
        {
            _logger.LogInformation("Sending a message to connection_id={id}", connectionId);

            var messageObject = new { content };

            var httpContent = new StringContent(messageObject.ToString(), Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = await _client.PostAsync($"connections/{connectionId}/send-message", httpContent);
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

        private async Task LogError(HttpResponseMessage response, Exception exception = null)
        {
            await LogError(null, response, exception);
        }

        private async Task LogError(HttpContent content, HttpResponseMessage response, Exception exception = null)
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

            _logger.LogError(exception, secondaryMessage, new Object[] { content, response });
        }
    }

    public class AcaPyApiException : Exception
    {
        public AcaPyApiException() : base() { }
        public AcaPyApiException(string message) : base(message) { }
        public AcaPyApiException(string message, Exception inner) : base(message, inner) { }
    }
}
