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
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Models;
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
        /// GetUserAccessAsync.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldGetUserAccess()
        {
            // Arrange
            HashSet<Common.Constants.BetaFeature> expectedBetaFeatures =
            [
                Common.Constants.BetaFeature.Salesforce,
            ];
            IBetaFeatureService service = SetupGetUserAccessMock();

            // Act
            UserBetaAccess actual = await service.GetUserAccessAsync(Email1);

            // Assert
            Assert.Single(actual.BetaFeatures);
            Assert.Equal(expectedBetaFeatures, actual.BetaFeatures);
        }

        /// <summary>
        /// GetUserAccessAsync.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetUserAccessThrowsException()
        {
            // Arrange
            IBetaFeatureService service = SetupGetUserAccessMock(false);

            // Act and Assert
            Exception exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.GetUserAccessAsync(Email1); });
            Assert.Equal(ErrorMessages.UserProfileNotFound, exception.Message);
        }

        /// <summary>
        /// GetAllUserAccessAsync.
        /// </summary>
        /// <param name="pageIndex">Current page index, starting from 0.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="expectedCount">Expected number of results.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 1, 1)]
        [InlineData(1, 1, 1)]
        [InlineData(0, 50, 2)] // SetupGetBetaFeatureAccessMock only generates 2 results in total
        [Theory]
        public async Task ShouldGetAllUserAccess(int pageIndex, int pageSize, int expectedCount)
        {
            // Arrange
            IList<BetaFeatureAccess> associations =
            [
                GenerateBetaFeatureAccess(Hdid1Email1, Email1),
                GenerateBetaFeatureAccess(Hdid2Email1, Email1),
                GenerateBetaFeatureAccess(Hdid3Email2, Email2),
            ];

            IGrouping<string, BetaFeatureAccess>[] groupedAssociations = [.. associations.GroupBy(a => a.UserProfile.Email!).OrderBy(a => a.Key)];
            IList<string> expectedEmails = groupedAssociations.Select(i => i.Key).ToList();
            IBetaFeatureService service = SetupGetBetaFeatureAccessMock(groupedAssociations);

            // Act
            PaginatedResult<UserBetaAccess> actual = await service.GetAllUserAccessAsync(pageIndex, pageSize);

            // Assert
            Assert.Equal(pageIndex, actual.PageIndex);
            Assert.Equal(pageSize, actual.PageSize);
            Assert.Equal(expectedCount, actual.Data.Count);

            List<string> expectedEmailAddresses = expectedEmails.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            Assert.Equal(expectedEmailAddresses.Count, actual.Data.Count);
            Assert.Equal(expectedEmailAddresses, actual.Data.Select(a => a.Email));
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
            HashSet<Common.Constants.BetaFeature> betaFeatures = betaFeature != null
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

            IList<BetaFeatureAccess> betaFeatureAssociations = existingBetaFeature != null
                ?
                [
                    GenerateBetaFeatureAccess(Hdid1Email1, Email1),
                    GenerateBetaFeatureAccess(Hdid2Email1, Email1),
                ]
                : [];

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

            BetaFeatureAccessMock mock = SetupBetaFeatureAccessMock(Hdid1Email1, Hdid2Email1, userProfiles, betaFeatureAssociations);

            // Act
            await mock.Service.SetUserAccessAsync(new() { Email = Email1, BetaFeatures = betaFeatures });

            // Assert
            mock.BetaFeatureAccessDelegateMock.Verify(
                v => v.DeleteRangeAsync(
                    It.Is<IEnumerable<BetaFeatureAccess>>(x => x.Count() == expectedDeletedCount),
                    false,
                    It.IsAny<CancellationToken>()));

            mock.BetaFeatureAccessDelegateMock.Verify(
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
            // Arrange
            HashSet<Common.Constants.BetaFeature> betaFeatures =
            [
                betaFeature, // Unknown can cause a not implemented exception
            ];

            IBetaFeatureService service = SetupSetUserAccessExceptionMock(Hdid2Email1, Email1, profileExists);

            // Act and Assert
            Exception exception = await Assert.ThrowsAsync(
                expectedExceptionType,
                async () => { await service.SetUserAccessAsync(new() { Email = Email1, BetaFeatures = betaFeatures }); });
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

        private static PaginatedResult<IGrouping<string, BetaFeatureAccess>> GeneratePaginatedResult(
            IList<IGrouping<string, BetaFeatureAccess>> data,
            int pageIndex,
            int pageSize)
        {
            return new()
            {
                Data = data.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = data.Count,
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
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static IBetaFeatureService SetupGetUserAccessMock(bool profileExists = true)
        {
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
            return GetBetaFeatureService(profileDelegateMock: profileDelegateMock);
        }

        private static IBetaFeatureService SetupGetBetaFeatureAccessMock(IGrouping<string, BetaFeatureAccess>[] groupedAssociations)
        {
            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock
                .Setup(s => s.GetAllAsync(It.Is<int>(i => i >= 0), It.Is<int>(i => i >= 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int pageIndex, int pageSize, CancellationToken _) => GeneratePaginatedResult(groupedAssociations, pageIndex, pageSize));
            return GetBetaFeatureService(betaFeatureAccessDelegateMock: betaFeatureAccessDelegateMock);
        }

        private static BetaFeatureAccessMock SetupBetaFeatureAccessMock(string hdid1, string hdid2, IList<UserProfile> userProfiles, IList<BetaFeatureAccess> betaFeatureAssociations)
        {
            IEnumerable<string> hdids = [hdid1, hdid2];
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfilesAsync(Email1, true, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock.Setup(s => s.GetAsync(hdids, It.IsAny<CancellationToken>())).ReturnsAsync(betaFeatureAssociations);
            IBetaFeatureService service = GetBetaFeatureService(profileDelegateMock, betaFeatureAccessDelegateMock);
            return new(service, betaFeatureAccessDelegateMock);
        }

        private static IBetaFeatureService SetupSetUserAccessExceptionMock(string hdid, string email, bool profileExists)
        {
            ICollection<BetaFeatureCode> betaFeatureCodes =
            [
                GenerateBetaFeatureCode(BetaFeature.Salesforce),
            ];

            IList<UserProfile> userProfiles = profileExists
                ? [GenerateUserProfile(hdid, email, betaFeatureCodes)]
                : []; // This can cause not found exception

            IEnumerable<string> hdids = [hdid];

            IList<BetaFeatureAccess> betaFeatureAssociations =
            [
                GenerateBetaFeatureAccess(hdid, email),
            ];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfilesAsync(email, true, It.IsAny<CancellationToken>())).ReturnsAsync(userProfiles);
            Mock<IBetaFeatureAccessDelegate> betaFeatureAccessDelegateMock = new();
            betaFeatureAccessDelegateMock.Setup(s => s.GetAsync(hdids, It.IsAny<CancellationToken>())).ReturnsAsync(betaFeatureAssociations);
            return GetBetaFeatureService(profileDelegateMock, betaFeatureAccessDelegateMock);
        }

        private static IBetaFeatureService GetBetaFeatureService(
            Mock<IUserProfileDelegate>? profileDelegateMock = null,
            Mock<IBetaFeatureAccessDelegate>? betaFeatureAccessDelegateMock = null)
        {
            profileDelegateMock ??= new();
            betaFeatureAccessDelegateMock ??= new();

            return new BetaFeatureService(
                profileDelegateMock.Object,
                betaFeatureAccessDelegateMock.Object,
                MappingService,
                new Mock<ILogger<BetaFeatureService>>().Object);
        }

        private sealed record BetaFeatureAccessMock(
            IBetaFeatureService Service,
            Mock<IBetaFeatureAccessDelegate> BetaFeatureAccessDelegateMock);
    }
}
