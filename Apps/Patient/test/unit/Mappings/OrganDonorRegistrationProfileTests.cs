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
namespace HealthGateway.PatientTests.Mappings
{
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.PatientDataAccess;
    using HealthGateway.PatientTests.Utils;
    using Xunit;
    using OrganDonorRegistration = HealthGateway.Patient.Services.OrganDonorRegistration;

    /// <summary>
    /// Organ Donor Registration Profile Unit Tests.
    /// </summary>
    public class OrganDonorRegistrationProfileTests
    {
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Should map patient data access organ donor registration to patient service organ donor registration.
        /// </summary>
        /// <param name="status">The patient data access organ donor registration status enum to test.</param>
        [Theory]
        [InlineData(OrganDonorRegistrationStatus.Error)]
        [InlineData(OrganDonorRegistrationStatus.Pending)]
        [InlineData(OrganDonorRegistrationStatus.NotRegistered)]
        [InlineData(OrganDonorRegistrationStatus.Registered)]
        public void ShouldGetOrganDonorRegistration(OrganDonorRegistrationStatus status)
        {
            // Arrange
            OrganDonorRegistration expected = new()
            {
                Id = string.Empty,
                Status = GetExpectedOrganDonorStatus(status),
                OrganDonorRegistrationLinkText = GetExpectedOrganDonorRegistrationLinkText(status),
                StatusMessage = string.Empty,
                RegistrationFileId = string.Empty,
            };

            PatientDataAccess.OrganDonorRegistration registration = new()
            {
                Status = status,
            };

            // Act
            OrganDonorRegistration actual = Mapper.Map<OrganDonorRegistration>(registration);

            // Verify
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.OrganDonorRegistrationLinkText, actual.OrganDonorRegistrationLinkText);
        }

        private static Patient.Models.OrganDonorRegistrationStatus GetExpectedOrganDonorStatus(OrganDonorRegistrationStatus status)
        {
            return status switch
            {
                OrganDonorRegistrationStatus.Registered => Patient.Models.OrganDonorRegistrationStatus.Registered,
                OrganDonorRegistrationStatus.NotRegistered => Patient.Models.OrganDonorRegistrationStatus.NotRegistered,
                OrganDonorRegistrationStatus.Error => Patient.Models.OrganDonorRegistrationStatus.Error,
                OrganDonorRegistrationStatus.Pending => Patient.Models.OrganDonorRegistrationStatus.Pending,
                _ => Patient.Models.OrganDonorRegistrationStatus.Unknown,
            };
        }

        private static string GetExpectedOrganDonorRegistrationLinkText(OrganDonorRegistrationStatus status)
        {
            return status switch
            {
                OrganDonorRegistrationStatus.Registered or
                    OrganDonorRegistrationStatus.Error or
                    OrganDonorRegistrationStatus.Pending => "If needed, you can update your decision",
                OrganDonorRegistrationStatus.NotRegistered => "Register as an organ donor and save lives",
                _ => EnumUtility.ToEnumString(Patient.Models.OrganDonorRegistrationStatus.Unknown),
            };
        }
    }
}
