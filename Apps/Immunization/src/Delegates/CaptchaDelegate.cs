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
namespace HealthGateway.Immunization.Delegates
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class CaptchaDelegate : ICaptchaDelegate
    {
        private const string CaptchaSectionConfigKey = "Captcha";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly CaptchaConfig captchaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptchaDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public CaptchaDelegate(
            ILogger<CaptchaDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.captchaConfig = new CaptchaConfig();
            IConfigurationSection? configSection = configuration?.GetSection(CaptchaSectionConfigKey);
            configSection.Bind(this.captchaConfig);
        }

        /// <inheritdoc/>
        public async Task<bool> IsCaptchaValid(string token)
        {
            var result = false;
            this.logger.LogTrace($"Validating captcha token {token}");

            try
            {
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                HttpResponseMessage response = await client.PostAsync(new Uri($"{this.captchaConfig.VerificationUrl}?secret={this.captchaConfig.SecretKey}&response={token}"), null!).ConfigureAwait(true);
                string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Validating captcha token response {jsonString}");
                CaptchaVerificationResponse? captchaVerfication = JsonSerializer.Deserialize<CaptchaVerificationResponse>(jsonString);
                result = captchaVerfication?.Success == true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // fail gracefully, but log
                this.logger.LogError("Failed to process captcha validation", e);
            }

            return result;
        }
    }
}
