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
namespace HealthGateway.Common.Utils.Odr
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.ODR;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Message handler that attaches the client certificate and basic authorization header required by ODR endpoints.
    /// </summary>
    public class OdrAuthorizationHandler : HttpClientHandler
    {
        private readonly string? base64AuthorizationString;

        /// <summary>
        /// Initializes a new instance of the <see cref="OdrAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        public OdrAuthorizationHandler(IConfiguration configuration)
        {
            OdrConfig odrConfig = new();
            configuration.Bind(OdrConfig.OdrConfigSectionKey, odrConfig);

            if (odrConfig.Authorization?.Enabled == true)
            {
                this.base64AuthorizationString = Convert.ToBase64String(Encoding.Default.GetBytes(odrConfig.Authorization?.Credentials));
            }

            if (odrConfig.ClientCertificate?.Enabled == true)
            {
                byte[] certificateData = File.ReadAllBytes(odrConfig.ClientCertificate?.Path);
                this.ClientCertificates.Add(new X509Certificate2(certificateData, odrConfig.ClientCertificate?.Password));
            }
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.base64AuthorizationString != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", this.base64AuthorizationString);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
