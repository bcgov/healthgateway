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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Utils;
    using HealthGateway.Immunization.Constants;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public class VaccineStatusService : IVaccineStatusService
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private const string AuthConfigSectionName = "ClientAuthentication";
        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly PHSAConfig phsaConfig;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="vaccineStatusDelegate">The injected vaccine status delegate.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate)
        {
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.authDelegate = authDelegate;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password

            this.phsaConfig = new PHSAConfig();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus(string phn, string dateOfBirth)
        {
            RequestResult<VaccineStatus> retVal = new ()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            DateTime dob;
            try
            {
                dob = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing date of birth",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            if (!PHNValidator.IsValid(phn))
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing phn",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            string? accessToken = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest).AccessToken;
            RequestResult<PHSAResult<VaccineStatusResult>> result =
                await this.vaccineStatusDelegate.GetVaccineStatus(phn, dob, accessToken).ConfigureAwait(true);
            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            retVal.ResultStatus = Common.Constants.ResultType.Success;

            if (payload == null)
            {
                retVal.ResourcePayload = new VaccineStatus();
                retVal.ResourcePayload.State = Constants.VaccineState.NotFound;
            }
            else
            {
                retVal.ResourcePayload = new VaccineStatus()
                {
                    Birthdate = payload.Birthdate,
                    PersonalHealthNumber = phn,
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    Doses = payload.DoseCount,
                    State = Enum.Parse<VaccineState>(payload.StatusIndicator),
                };
            }

            if (result.ResourcePayload != null)
            {
                retVal.ResourcePayload.Loaded = !result.ResourcePayload.LoadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(result.ResourcePayload.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
            }

            return retVal;
        }
    }
}
