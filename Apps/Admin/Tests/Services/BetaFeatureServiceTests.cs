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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// BetaFeatureService's Unit Tests.
    /// </summary>
    public class BetaFeatureServiceTests
    {
        private const string Hdid1Email1 = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const string Hdid2Email1 = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private const string Hdid3Email2 = "RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ";
        private const string Email1 = "Email1@gov.bc.ca";
        private const string Email2 = "Email2@gov.bc.ca";

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// GetUserProfilesAsync.
        /// </summary>
        /// <param name="profileExists">Value to determine whether user profile exists or not.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task ShouldGetUserAccess(bool profileExists)
        {
            // Arrange
            IEnumerable<Common.Constants.BetaFeature> expected =
            [
                Common.Constants.BetaFeature.Salesforce,
            ];

            ICollection<BetaFeatureCode> betaFeatureCodes =
            [
                GenerateBetaFeatureCode(BetaFeature.Salesforce),
            ];

            IList<UserProfile> userProfiles = profileExists
                ?
                [
                    GenerateUserProfile(Hdid1Email1, Email1, betaFeatureCodes),
                    GenerateUserProfile(Hdid2Email1, Email1, betaFeatureCodes),
                ]
                : [];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfilesAsync(Email1, true, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            IBetaFeatureService service = GetBetaFeatureService(profileDelegateMock: profileDelegateMock);

            if (profileExists)
            {
                // Act
                IEnumerable<Common.Constants.BetaFeature> enumerable = await service.GetUserAccessAsync(Email1);
                IList<Common.Constants.BetaFeature> actual = enumerable.ToList();

                // Assert
                Assert.Single(actual);
                Assert.Equal(expected, actual);
            }
            else
            {
                // Act and Assert
                Exception exception = await Assert.ThrowsAsync<NotFoundException>(
                    async () => { await service.GetUserAccessAsync(Email1); });
                Assert.Equal(ErrorMessages.UserProfileNotFound, exception.Message);
            }
        }

        /// <summary>
        /// GetBetaFeatureAccessAsync.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldGetBetaFeatureAccess()
        {
            // Arrange
            ICollection<BetaFeatureCode> betaFeatureCodes =
            [
                GenerateBetaFeatureCode(BetaFeature.Salesforce),
            ];

            IList<BetaFeatureAccess> betaFeatureAssociations =
            [
                GenerateBetaFeatureAccess(Hdid1Email1, Email1),
                GenerateBetaFeatureAccess(Hdid2Email1, Email1),
                GenerateBetaFeatureAccess(Hdid3Email2, Email2),
            ];

            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock.Setup(s => s.GetAllAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(betaFeatureAssociations);
            IBetaFeatureService service = GetBetaFeatureService(betaFeatureAccessDelegateMock: betaFeatureAccessDelegateMock);

            const int expectedCount = 2;

            // Act
            IEnumerable<Common.Models.BetaFeatureAccess> enumerable = await service.GetBetaFeatureAccessAsync();
            IList<Common.Models.BetaFeatureAccess> actual = enumerable.ToList();

            // Assert
            Assert.Equal(expectedCount, actual.Count);
            Assert.Equal(Email1, actual.ElementAt(0).Email);
            Assert.Equal(Email2, actual.ElementAt(1).Email);
        }

        /// <summary>
        /// SetUserAccessAsync.
        /// </summary>
        /// <param name="existingBetaFeature">User profile's current associated beta feature.</param>
        /// <param name="betaFeature">Beta feature to associate to user profile.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(BetaFeature.Salesforce, Common.Constants.BetaFeature.Salesforce)]
        [InlineData(null, Common.Constants.BetaFeature.Salesforce)]
        [InlineData(BetaFeature.Salesforce, null)]
        [InlineData(null, null)]
        [Theory]
        public async Task ShouldSetUserAccess(BetaFeature? existingBetaFeature, Common.Constants.BetaFeature? betaFeature)
        {
            // Arrange
            IList<Common.Constants.BetaFeature> betaFeatures = betaFeature != null
                ? [betaFeature.Value]
                : [];

            ICollection<BetaFeatureCode> betaFeatureCodes = existingBetaFeature != null
                ? [GenerateBetaFeatureCode(existingBetaFeature.Value)]
                : [];

            IList<UserProfile> userProfiles =
            [
                GenerateUserProfile(Hdid1Email1, Email1, betaFeatureCodes),
                GenerateUserProfile(Hdid2Email1, Email1, betaFeatureCodes),
            ];

            IEnumerable<string> hdids = [Hdid1Email1, Hdid2Email1];

            IList<BetaFeatureAccess> betaFeatureAssociations = existingBetaFeature != null
                ?
                [
                    GenerateBetaFeatureAccess(Hdid1Email1, Email1),
                    GenerateBetaFeatureAccess(Hdid2Email1, Email1),
                ]
                : [];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfilesAsync(Email1, true, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock.Setup(s => s.GetAsync(hdids, It.IsAny<CancellationToken>())).ReturnsAsync(betaFeatureAssociations);
            IBetaFeatureService service = GetBetaFeatureService(profileDelegateMock, betaFeatureAccessDelegateMock);

            int expectedDeletedCount = 0;
            if (existingBetaFeature != null && betaFeature == null)
            {
                // Right now there is only one Salesforce feature. When there are more features, this count can be different if test setup is changed.
                expectedDeletedCount = betaFeatureAssociations.Count;
            }

            int expectedAddedCount = 0;
            if (existingBetaFeature == null && betaFeature != null)
            {
                // Right now there is only one Salesforce feature. When there are more features, this count can be different if test setup is changed.
                expectedAddedCount = userProfiles.Count;
            }

            // Act
            await service.SetUserAccessAsync(Email1, betaFeatures);

            // Assert
            betaFeatureAccessDelegateMock.Verify(
                v => v.DeleteRangeAsync(
                    It.Is<IEnumerable<BetaFeatureAccess>>(x => x.Count() == expectedDeletedCount),
                    false,
                    It.IsAny<CancellationToken>()));

            betaFeatureAccessDelegateMock.Verify(
                v => v.AddRangeAsync(
                    It.Is<IEnumerable<BetaFeatureAccess>>(x => x.Count() == expectedAddedCount),
                    true,
                    It.IsAny<CancellationToken>()));
        }

        /// <summary>
        /// SetUserAccessAsync throws exception.
        /// </summary>
        /// <param name="expectedExceptionType">The exception type to be thrown.</param>
        /// <param name="expectedErrorMessage">The associated error message for the exception.</param>
        /// <param name="profileExists">Value to determine whether user profile exists or not.</param>
        /// <param name="betaFeature">The beta feature to set.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(typeof(NotImplementedException), "Reverse mapping for Unknown is not implemented", true, Common.Constants.BetaFeature.Unknown)]
        [InlineData(typeof(NotFoundException), ErrorMessages.UserProfileNotFound, false, Common.Constants.BetaFeature.Salesforce)]
        [Theory]
        public async Task SetUserAccessThrowsException(Type expectedExceptionType, string expectedErrorMessage, bool profileExists, Common.Constants.BetaFeature betaFeature)
        {
            IList<Common.Constants.BetaFeature> betaFeatures =
            [
                betaFeature,
            ];

            // Arrange
            ICollection<BetaFeatureCode> betaFeatureCodes =
            [
                GenerateBetaFeatureCode(BetaFeature.Salesforce),
            ];

            IList<UserProfile> userProfiles = profileExists
                ? [GenerateUserProfile(Hdid1Email1, Email1, betaFeatureCodes)]
                : [];

            IEnumerable<string> hdids = [Hdid1Email1];

            IList<BetaFeatureAccess> betaFeatureAssociations =
            [
                GenerateBetaFeatureAccess(Hdid1Email1, Email1),
            ];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfilesAsync(Email1, true, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock.Setup(s => s.GetAsync(hdids, It.IsAny<CancellationToken>())).ReturnsAsync(betaFeatureAssociations);
            IBetaFeatureService service = GetBetaFeatureService(profileDelegateMock, betaFeatureAccessDelegateMock);

            // Act and Assert
            Exception exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => { await service.SetUserAccessAsync(Email1, betaFeatures); });
            Assert.Equal(expectedErrorMessage, exception.Message);
        }

        private static UserProfile GenerateUserProfile(string hdid, string email, ICollection<BetaFeatureCode>? betaFeatureCodes)
        {
            return new()
            {
                HdId = hdid,
                Email = email,
                BetaFeatureCodes = betaFeatureCodes,
            };
        }

        private static BetaFeatureCode GenerateBetaFeatureCode(BetaFeature betaFeature)
        {
            return new BetaFeatureCode
            {
                Code = betaFeature,
            };
        }

        private static BetaFeatureAccess GenerateBetaFeatureAccess(string hdid, string email)
        {
            return new()
            {
                Hdid = hdid,
                BetaFeatureCode = BetaFeature.Salesforce,
                UserProfile = new()
                {
                    HdId = Hdid1Email1,
                    Email = email,
                },
            };
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
                { "EnabledRoles", "[ \"AdminUser\", \"AdminReviewer\", \"SupportUser\" ]" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IBetaFeatureService GetBetaFeatureService(
            Mock<IUserProfileDelegate>? profileDelegateMock = null,
            Mock<IBetaFeatureAccessDelegate>? betaFeatureAccessDelegateMock = null)
        {
            profileDelegateMock = profileDelegateMock ?? new();
            betaFeatureAccessDelegateMock = betaFeatureAccessDelegateMock ?? new();

            return new BetaFeatureService(
                profileDelegateMock.Object,
                betaFeatureAccessDelegateMock.Object,
                MappingService,
                new Mock<ILogger<BetaFeatureService>>().Object);
        }
    }
}
