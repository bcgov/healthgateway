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
namespace HealthGateway.GatewayApiTests.Services.Test.Mock
{
    using System.Collections.Generic;
    using System.Threading;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Moq;

    /// <summary>
    /// UserProfileDelegateMock.
    /// </summary>
    public class UserProfileDelegateMock : Mock<IUserProfileDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileDelegateMock"/> class.
        /// </summary>
        /// <param name="userProfile">user profile.</param>
        /// <param name="userProfileData">user profile data.</param>
        /// <param name="hdid">hdid.</param>
        /// <param name="userProfileHistoryList">list of user profile history.</param>
        /// <param name="limit">limit.</param>
        /// <param name="updatedUserProfileResult">updated user profile result.</param>
        public UserProfileDelegateMock(
            UserProfile userProfile,
            DbResult<UserProfile> userProfileData,
            string hdid,
            IList<UserProfileHistory> userProfileHistoryList,
            int limit,
            DbResult<UserProfile> updatedUserProfileResult)
        {
            this.Setup(s => s.GetUserProfileAsync(hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfileData.Payload);
            this.Setup(s => s.UpdateAsync(userProfile, true, It.IsAny<CancellationToken>())).ReturnsAsync(updatedUserProfileResult);
            this.Setup(s => s.GetUserProfileHistoryListAsync(hdid, limit, It.IsAny<CancellationToken>())).ReturnsAsync(userProfileHistoryList);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileDelegateMock"/> class.
        /// </summary>
        /// <param name="userProfile">user profile.</param>
        /// <param name="insertResult">insert result.</param>
        /// <param name="commit">commit.</param>
        public UserProfileDelegateMock(UserProfile userProfile, DbResult<UserProfile> insertResult, bool commit = true)
        {
            this.Setup(s => s.InsertUserProfileAsync(It.Is<UserProfile>(x => x.HdId == userProfile.HdId), commit, CancellationToken.None)).ReturnsAsync(insertResult);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileDelegateMock"/> class.
        /// </summary>
        /// <param name="hdid">hdid.</param>
        /// <param name="userProfile">user profile.</param>
        /// <param name="userProfileDbResult">user profile from DBResult.</param>
        /// <param name="commit">commit.</param>
        /// <param name="userProfileUpdateDbResult">Optional update result.</param>
        public UserProfileDelegateMock(string hdid, UserProfile userProfile, DbResult<UserProfile> userProfileDbResult, bool commit = true, DbResult<UserProfile>? userProfileUpdateDbResult = null)
        {
            this.Setup(s => s.GetUserProfileAsync(hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfileDbResult.Payload);
            this.Setup(s => s.UpdateAsync(userProfile, commit, It.IsAny<CancellationToken>())).ReturnsAsync(userProfileUpdateDbResult ?? userProfileDbResult);
        }
    }
}
