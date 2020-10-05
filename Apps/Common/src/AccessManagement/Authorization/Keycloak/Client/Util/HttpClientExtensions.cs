//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;

    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak;
    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;


/// <summary>Extensions for HttpClient to handle OAuth 2.0 UMA.</summary>
    public static class HttpClientExtensions
    {
        /// <summary>Sets the Bearer Token Head for UMA calls</summary>
        /// <param name="httpClient">The target of this extension method.</param>
        /// <param name="base64BearerToken">The OAuth 2 Access Token base 64 encoded</param>
        public static void BearerTokenAuthorization(this HttpClient httpClient, string base64BearerToken)
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", @"Bearer " + base64BearerToken);
        }

        /// <summary>Sets the UMA multipart form parameters from teh AuthorizationRequest provided.</summary>
        /// <param name="httpClient">The target of this extension method.</param>
        /// <param name="request">An <cref name="AuthorizationRequest"/> request.</param>
        public static void Uma(this HttpClient httpClient, AuthorizationRequest request)
        {
            string? ticket = request.Ticket;
            PermissionTicketToken? permissionTicketToken = request.Permissions;

            if (ticket == null && permissionTicketToken == null) {
                throw new Exception("You must either provide a permission ticket or the permissions you want to request.");
            }

            MultipartFormDataContent multiForm = new MultipartFormDataContent();

            multiForm.Add(new StringContent(OAuth2Constants.UMA_GRANT_TYPE), OAuth2Constants.GRANT_TYPE);
            multiForm.Add(new StringContent(ticket), "ticket");
            multiForm.Add(new StringContent(request.ClaimToken), "claim_token");
            multiForm.Add(new StringContent(request.ClaimTokenFormat), "claim_token_format");
            multiForm.Add(new StringContent(request.Pct), "pct");
            multiForm.Add(new StringContent(request.RptToken), "rpt");
            multiForm.Add(new StringContent(request.Scope), "scope");
            multiForm.Add(new StringContent(request.Audience), "audience");
            multiForm.Add(new StringContent(request.SubjectToken), "subject_token");

            if (permissionTicketToken!.Permissions != null)
            {
                foreach (Permission permission in permissionTicketToken.Permissions)
                {
                    string resourceId = permission.ResourceId;
                    List<string>? scopes = permission.Scopes;
                    StringBuilder value = new StringBuilder();

                    if (resourceId != null)
                    {
                        value.Append(resourceId);
                    }

                    if (scopes != null && (scopes.Count > 0))
                    {
                        value.Append("#");
                        foreach (string scope in scopes)
                        {
                            if (!value.ToString().EndsWith("#"))
                            {
                                value.Append(",");
                            }
                            value.Append(scope);
                        }
                    }
                    multiForm.Add(new StringContent(value.ToString()), "permission");
                }
            }

            Metadata? metadata = request.Metadata;

            if (metadata != null)
            {
                if (metadata.IncludeResourceName)
                {
                    multiForm.Add(new StringContent(metadata.IncludeResourceName.ToString()), "response_include_resource_name");
                }

                if (metadata.Limit > 0)
                {
                    multiForm.Add(new StringContent(metadata.Limit.ToString()), "response_permissions_limit");
                }
            }
        }
    }
}