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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    /// <summary>
    /// Initializes a new instance of the <see cref="UserValidationService"/> class.
    /// </summary>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="patientDetailsService">The injected patient details service.</param>
    public class UserValidationService(IConfiguration configuration, IPatientDetailsService patientDetailsService) : IUserValidationService
    {
        private const string MinPatientAgeKey = "MinPatientAge";
        private const string WebClientConfigSection = "WebClient";
        private const int DefaultPatientAge = 12;
        private readonly int minPatientAge = configuration.GetSection(WebClientConfigSection).GetValue(MinPatientAgeKey, DefaultPatientAge);

        /// <inheritdoc/>
        public async Task<bool> IsPhoneNumberValidAsync(string phoneNumber, CancellationToken ct = default)
        {
            return (await new SmsNumberValidator().ValidateAsync(phoneNumber, ct)).IsValid;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateEligibilityAsync(string hdid, CancellationToken ct = default)
        {
            if (this.minPatientAge == 0)
            {
                return true;
            }

            PatientDetails patient = await patientDetailsService.GetPatientAsync(hdid, ct: ct);
            return (await new AgeRangeValidator(this.minPatientAge).ValidateAsync(patient.Birthdate.ToDateTime(TimeOnly.MinValue), ct)).IsValid;
        }
    }
}
