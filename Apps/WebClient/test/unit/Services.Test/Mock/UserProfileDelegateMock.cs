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
namespace HealthGateway.WebClientTests.Services.Test.Mock
{
    using System;
    using HealthGateway.Database.Constants;
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
        public UserProfileDelegateMock(UserProfile userProfile, DBResult<UserProfile> userProfileData, string hdid)
        {
            this.Setup(s => s.GetUserProfile(hdid)).Returns(userProfileData);
            this.Setup(s => s.Update(userProfile, true)).Returns(userProfileData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileDelegateMock"/> class.
        /// </summary>
        /// <param name="userProfile">user profile.</param>
        /// <param name="insertResult">insert result.</param>
        public UserProfileDelegateMock(UserProfile userProfile, DBResult<UserProfile> insertResult)
        {
            this.Setup(s => s.InsertUserProfile(It.Is<UserProfile>(x => x.HdId == userProfile.HdId))).Returns(insertResult);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileDelegateMock"/> class.
        /// </summary>
        /// <param name="hdid">hdid.</param>
        /// <param name="userProfile">user profile.</param>
        /// <param name="userProfileDBResult">user profile from DBResult.</param>
        /// <param name="commit">commit.</param>
        public UserProfileDelegateMock(string hdid, UserProfile userProfile, DBResult<UserProfile> userProfileDBResult, bool commit = true)
        {
            this.Setup(s => s.GetUserProfile(hdid)).Returns(userProfileDBResult);
            this.Setup(s => s.Update(userProfile, commit)).Returns(userProfileDBResult);
        }
    }
}
