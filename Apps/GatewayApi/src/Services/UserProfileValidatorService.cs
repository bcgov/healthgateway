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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class UserProfileValidatorService : IUserProfileValidatorService
    {
        private const string WebClientConfigSection = "WebClient";
        private const string MinPatientAgeKey = "MinPatientAge";
        private readonly int minPatientAge;

        private readonly IPatientService patientService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileValidatorService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="patientService">The patient service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public UserProfileValidatorService(ILogger<UserProfileValidatorService> logger, IPatientService patientService, IConfiguration configuration)
        {
            this.logger = logger;
            this.patientService = patientService;
            this.minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, 12);
        }

        /// <inheritdoc/>
        public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await UserProfileValidator.ValidateUserProfileSmsNumberAsync(phoneNumber, ct);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<bool>> ValidateMinimumAgeAsync(string hdid, CancellationToken ct = default)
        {
            if (this.minPatientAge == 0)
            {
                return RequestResultFactory.Success(true);
            }

            RequestResult<PatientModel> patientResult = await this.patientService.GetPatientAsync(hdid, ct: ct);

            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                this.logger.LogWarning("Error retrieving patient age... {Hdid}", hdid);
                return RequestResultFactory.Error(false, patientResult.ResultError);
            }

            bool isValid = AgeRangeValidator.IsValid(patientResult.ResourcePayload.Birthdate, this.minPatientAge);

            return RequestResultFactory.Success(isValid);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserProfileModel>?> ValidateUserProfileAsync(CreateUserRequest createProfileRequest, CancellationToken ct = default)
        {
            // Validate registration age
            string hdid = createProfileRequest.Profile.HdId;
            RequestResult<bool> isMinimumAgeResult = await this.ValidateMinimumAgeAsync(hdid, ct);
            if (isMinimumAgeResult.ResultStatus != ResultType.Success)
            {
                return RequestResultFactory.Error<UserProfileModel>(isMinimumAgeResult.ResultError);
            }

            if (!isMinimumAgeResult.ResourcePayload)
            {
                this.logger.LogWarning("Patient under minimum age... {Hdid}", createProfileRequest.Profile.HdId);
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.InvalidState, "Patient under minimum age");
            }

            // Validate UserProfile inputs
            if (!await UserProfileValidator.ValidateUserProfileSmsNumberAsync(createProfileRequest.Profile.SmsNumber, ct))
            {
                this.logger.LogWarning("Profile inputs have failed validation for {Hdid}", createProfileRequest.Profile.HdId);
                return RequestResultFactory.Error<UserProfileModel>(ErrorType.SmsInvalid, "Profile values entered are invalid");
            }

            return null;
        }
    }
}
