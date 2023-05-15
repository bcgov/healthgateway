// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Utils
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Delegating access handler that sets the bearer token to a configurable value.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StaticAuthHeaderHandler : DelegatingHandler
    {
        private readonly string apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticAuthHeaderHandler"/> class.
        /// </summary>
        /// <param name="apiKey">The api key to add to the request.</param>
        public StaticAuthHeaderHandler(string apiKey)
        {
            this.apiKey = apiKey;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"ApiKey-v1 {this.apiKey}");
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
