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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models;
    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models.Tokens;

    using Refit;


    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    [Headers("Authorization: Bearer")]   // The requesting party token as extension to Bearer token
    public interface IProtectionResource
    {
        /// <summary>Introspects the given rpt using the token introspection endpoint.</summary>
        /// <param name="request">the Requesting Party Token to Introspect along with the token hint.</param>
        /// <returns>A TokenIntrospectionResponse.</returns>
        [Post("/protocol/openid-connect/token/introspect")]
        public Task<TokenIntrospectionResponse> IntrospectRequestingPartyToken([Body(BodySerializationMethod.UrlEncoded)] TokenIntrospectionRequest request);
    }
}