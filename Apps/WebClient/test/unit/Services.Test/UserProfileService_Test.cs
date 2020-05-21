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
namespace HealthGateway.WebClient.Test.Services
{
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Constant;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Common.Constants;

    public class UserProfileServiceTest
    {
        [Fact]
        public void ShouldGetUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";
            UserProfile userProfile = new UserProfile
            {
                HdId = hdid,
                AcceptedTermsOfService = true,
            };

            DBResult<UserProfile> userProfileDBResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = DBStatusCode.Read
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            LegalAgreement termsOfService = new LegalAgreement()
            {
                Id = Guid.NewGuid(),
                LegalText = "",
                EffectiveDate = DateTime.Now
            };

            Mock<IEmailQueueService> emailer = new Mock<IEmailQueueService>();
            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(userProfileDBResult);
            profileDelegateMock.Setup(s => s.Update(userProfile, true)).Returns(userProfileDBResult);

            Mock<IEmailDelegate> emailDelegateMock = new Mock<IEmailDelegate>();
            Mock<IEmailInviteDelegate> emailInviteDelegateMock = new Mock<IEmailInviteDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetByInviteKey(It.IsAny<Guid>())).Returns(new EmailInvite());

            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration());

            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new Mock<ILegalAgreementDelegate>();
            legalAgreementDelegateMock
                .Setup(s => s.GetActiveByAgreementType(AgreementType.TermsofService))
                .Returns(new DBResult<LegalAgreement>() { Payload = termsOfService });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                profileDelegateMock.Object,
                emailDelegateMock.Object,
                emailInviteDelegateMock.Object,
                configServiceMock.Object,
                emailer.Object,
                legalAgreementDelegateMock.Object,
                cryptoDelegateMock.Object,
                notificationServiceMock.Object
            );
            RequestResult<UserProfileModel> actualResult = service.GetUserProfile(hdid);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(expected));
        }

        [Fact]
        public void ShouldInsertUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = "1234567890123456789012345678901234567890123456789012",
                AcceptedTermsOfService = true,
                Email = string.Empty
            };

            DBResult<UserProfile> insertResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = DBStatusCode.Created
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            Mock<IEmailQueueService> emailer = new Mock<IEmailQueueService>();
            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.InsertUserProfile(userProfile)).Returns(insertResult);

            Mock<IEmailDelegate> emailDelegateMock = new Mock<IEmailDelegate>();
            Mock<IEmailInviteDelegate> emailInviteDelegateMock = new Mock<IEmailInviteDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetByInviteKey(It.IsAny<Guid>())).Returns(new EmailInvite());

            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration() { WebClient = new WebClientConfiguration() { RegistrationStatus = RegistrationStatus.Open } });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns("abc");

            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            notificationServiceMock.Setup(s => s.SendNotificationSettings(It.IsAny<NotificationSettingsRequest>(), "bearer_token"))
                .ReturnsAsync(new RequestResult<NotificationSettingsResponse>() { ResultStatus = ResultType.Success });

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                profileDelegateMock.Object,
                emailDelegateMock.Object,
                emailInviteDelegateMock.Object,
                configServiceMock.Object,
                emailer.Object,
                new Mock<ILegalAgreementDelegate>().Object,
                cryptoDelegateMock.Object,
                notificationServiceMock.Object);


            RequestResult<UserProfileModel> actualResult = service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, new System.Uri("http://localhost/"), "bearer_token");

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(expected));
        }

        [Fact]
        public void ShouldQueueNotificationUpdate()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = "1234567890123456789012345678901234567890123456789012",
                AcceptedTermsOfService = true,
                Email = string.Empty
            };

            DBResult<UserProfile> insertResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = DBStatusCode.Created
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            Mock<IEmailQueueService> emailer = new Mock<IEmailQueueService>();
            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.InsertUserProfile(userProfile)).Returns(insertResult);

            Mock<IEmailDelegate> emailDelegateMock = new Mock<IEmailDelegate>();
            Mock<IEmailInviteDelegate> emailInviteDelegateMock = new Mock<IEmailInviteDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetByInviteKey(It.IsAny<Guid>())).Returns(new EmailInvite());

            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration() { WebClient = new WebClientConfiguration() { RegistrationStatus = RegistrationStatus.Open } });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns("abc");

            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            notificationServiceMock.Setup(s => s.SendNotificationSettings(It.IsAny<NotificationSettingsRequest>(), "bearer_token"))
                .ReturnsAsync(new RequestResult<NotificationSettingsResponse>() { ResultStatus = ResultType.Error });
            notificationServiceMock.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>(), "bearer_token"));


            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                profileDelegateMock.Object,
                emailDelegateMock.Object,
                emailInviteDelegateMock.Object,
                configServiceMock.Object,
                emailer.Object,
                new Mock<ILegalAgreementDelegate>().Object,
                cryptoDelegateMock.Object,
                notificationServiceMock.Object);

            RequestResult<UserProfileModel> actualResult = service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, new System.Uri("http://localhost/"), "bearer_token");
            notificationServiceMock.Verify(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>(), "bearer_token"), Times.Once());
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(expected));
        }
    }
}
