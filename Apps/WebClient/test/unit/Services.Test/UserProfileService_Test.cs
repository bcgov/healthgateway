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

    public class UserProfileServiceTest
    {
        [Fact]
        public void ShouldGetUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";
            UserProfile userProfile = new UserProfile
            {
                HdId = hdid,
                AcceptedTermsOfService = true
            };

            DBResult<UserProfile> expected = new DBResult<UserProfile> {
                Payload = userProfile,
                Status = Database.Constant.DBStatusCode.Read
            };

            Mock<IEmailQueueService> emailer = new Mock<IEmailQueueService>();
            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(expected);
            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                profileDelegateMock.Object,
                emailer.Object);
            DBResult<UserProfile> actualResult = service.GetUserProfile(hdid);

            Assert.Equal(Database.Constant.DBStatusCode.Read, actualResult.Status);
            Assert.True(actualResult.IsDeepEqual(expected)); 
        }

        [Fact]
        public void ShouldInsertUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = "1234567890123456789012345678901234567890123456789012",
                AcceptedTermsOfService = true
            };

            DBResult<UserProfile> expected = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = Database.Constant.DBStatusCode.Created
            };

            Mock<IEmailQueueService> emailer = new Mock<IEmailQueueService>();
            // emailer.Setup(s => s.QueueEmail(
            Mock<IProfileDelegate> profileDelegateMock = new Mock<IProfileDelegate>();
            profileDelegateMock.Setup(s => s.InsertUserProfile(userProfile)).Returns(expected);
            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                profileDelegateMock.Object,
                emailer.Object);
            DBResult<UserProfile> actualResult = service.CreateUserProfile(userProfile, new System.Uri("http://localhost/"));

            Assert.Equal(Database.Constant.DBStatusCode.Created, actualResult.Status);
            Assert.True(actualResult.IsDeepEqual(expected));
        }
    }
}
