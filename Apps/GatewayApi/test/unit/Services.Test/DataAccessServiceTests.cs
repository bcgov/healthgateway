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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// DataAccessServiceTests unit tests.
    /// </summary>
    public class DataAccessServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string DelegateHdid = "delegate-hdid-mock";
        private const string DelegatedHdid = DelegateHdid;
        private const string NotDelegatedHdid = "not-delegated-hdid-mock";
        private const string SubjectHdid = "subject-hdid-mock";
        private const string Email = "email-mock";
        private const string Sms = "sms-mock";

        /// <summary>
        /// GetBlockedDatasetsAsync.
        /// </summary>
        /// <param name="blockedDatasetsExists">The value indicating whether user blocked datasets exists or not.</param>
        /// <param name="reason">The reason or description for the specific test case scenario.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, "Returns blocked data sets for specified user.")]
        [InlineData(false, "Returns empty array as there are no blocked data sets for specified user.")]
        public async Task ShouldGetBlockedDatasets(bool blockedDatasetsExists, string reason)
        {
            // Arrange
            DataSource[] blockedDataSources = blockedDatasetsExists ? [DataSource.Immunization, DataSource.Medication] : [];

            BlockedDatasets expected = new()
            {
                Hdid = Hdid,
                DataSources = blockedDataSources,
            };

            Mock<IBlockedAccessDelegate> blockedAccessDelegateMock = new();
            blockedAccessDelegateMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(blockedDataSources);

            IDataAccessService service = GetDataAccessService(blockedAccessDelegateMock);

            // Act
            BlockedDatasets actual = await service.GetBlockedDatasetsAsync(Hdid, CancellationToken.None);

            // Assert
            actual.Should().BeEquivalentTo(expected, reason);
        }

        /// <summary>
        /// GetContactInfoAsync.
        /// </summary>
        /// <param name="profileEmailExists">The value indicating whether user profile email exists or not.</param>
        /// <param name="profileSmsExists">The value indicating whether user profile sms exists or not.</param>
        /// <param name="verificationEmailExists">The value indicating whether verification email exists or not.</param>
        /// <param name="verificationSmsExists">The value indicating whether verification sms exists or not.</param>
        /// <param name="reason">The reason or description for the specific test case scenario.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, false, false, "Return contact info with email and phone from user profile.")]
        [InlineData(false, false, true, true, "Return contact info with email and phone from messaging verification.")]
        [InlineData(false, false, false, false, "Return contact info with neither email nor phone populated.")]
        public async Task ShouldGetContactInfo(bool profileEmailExists, bool profileSmsExists, bool verificationEmailExists, bool verificationSmsExists, string reason)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = profileEmailExists ? Email : null,
                SmsNumber = profileSmsExists ? Sms : null,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            MessagingVerification[] verifications = CreateVerifications(verificationEmailExists, verificationSmsExists);

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            foreach (MessagingVerification verification in verifications)
            {
                messagingVerificationDelegateMock.Setup(
                        s => s.GetLastForUserAsync(
                            It.IsAny<string>(),
                            It.Is<string>(x => x == verification.VerificationType),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(verification);
            }

            ContactInfo expected = new()
            {
                Hdid = Hdid,
                Email = profileEmailExists || verificationEmailExists ? Email : null,
                Phone = profileSmsExists || verificationSmsExists ? Sms : null,
            };

            IDataAccessService service = GetDataAccessService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock);

            // Act
            ContactInfo actual = await service.GetContactInfoAsync(Hdid, CancellationToken.None);

            // Assert
            actual.Should().BeEquivalentTo(expected, reason);
            return;

            // Local helper
            static MessagingVerification[] CreateVerifications(bool verificationEmailExists, bool verificationSmsExists)
            {
                MessagingVerification emailVerification = new()
                {
                    VerificationType = MessagingVerificationType.Email,
                    Email = verificationEmailExists
                        ? new()
                        {
                            To = Email,
                        }
                        : null,
                };

                MessagingVerification smsVerification = new()
                {
                    VerificationType = MessagingVerificationType.Sms,
                    SmsNumber = verificationSmsExists ? Sms : null,
                };

                return [emailVerification, smsVerification];
            }
        }

        /// <summary>
        /// GetContactInfoAsync throws not found exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetContactInfoThrowsNotFoundException()
        {
            // Arrange
            UserProfile? userProfile = null;

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            IDataAccessService service = GetDataAccessService(userProfileDelegateMock: userProfileDelegateMock);

            // Act and Assert
            await Assert.ThrowsAsync<NotFoundException>(
                async () => { await service.GetContactInfoAsync(Hdid, CancellationToken.None); });
        }

        /// <summary>
        /// GetUserProtectionAsync.
        /// </summary>
        /// <param name="isSubjectProtected">The value indicating whether subject is protected or not.</param>
        /// <param name="subjectHdid">The subject's HDID.</param>
        /// <param name="delegateHdid">The delegate's HDID.</param>
        /// <param name="reason">The reason or description for the specific test case scenario.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, SubjectHdid, DelegateHdid, "Returns true for protected subject.")]
        [InlineData(true, SubjectHdid, NotDelegatedHdid, "Returns false for protected subject because invalid delegate.")]
        [InlineData(false, SubjectHdid, DelegateHdid, "Returns false for protected subject because subject is not protected.")]
        public async Task ShouldGetUserProtection(bool isSubjectProtected, string subjectHdid, string delegateHdid, string reason)
        {
            // Arrange
            Dependent dependent = new()
            {
                HdId = subjectHdid,
                Protected = isSubjectProtected,
                AllowedDelegations = isSubjectProtected ? [new() { DependentHdId = subjectHdid, DelegateHdId = DelegatedHdid }] : [],
            };

            Mock<IDelegationDelegate> delegationDelegateMock = new();
            delegationDelegateMock.Setup(
                    s => s.GetDependentAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dependent);

            UserProtection expected = new()
            {
                DelegateHdid = delegateHdid,
                ProtectedSubject = new(subjectHdid, isSubjectProtected && delegateHdid == DelegatedHdid),
            };

            IDataAccessService service = GetDataAccessService(delegationDelegateMock: delegationDelegateMock);

            // Act
            UserProtection actual = await service.GetUserProtectionAsync(subjectHdid, delegateHdid, CancellationToken.None);

            // Assert
            actual.Should().BeEquivalentTo(expected, reason);
        }

        private static IDataAccessService GetDataAccessService(
            Mock<IBlockedAccessDelegate>? blockedAccessDelegateMock = null,
            Mock<IDelegationDelegate>? delegationDelegateMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null)
        {
            blockedAccessDelegateMock ??= new();
            delegationDelegateMock ??= new();
            messagingVerificationDelegateMock ??= new();
            userProfileDelegateMock ??= new();

            return new DataAccessService(
                blockedAccessDelegateMock.Object,
                delegationDelegateMock.Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                new Mock<ILogger<DataAccessService>>().Object);
        }
    }
}
