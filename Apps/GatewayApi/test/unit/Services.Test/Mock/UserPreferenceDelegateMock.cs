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
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApiTests.Services.Test.Constants;
    using Moq;

    /// <summary>
    /// UserPreferenceDelegateMock.
    /// </summary>
    public class UserPreferenceDelegateMock : Mock<IUserPreferenceDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDelegateMock"/> class.
        /// </summary>
        /// <param name="userPreferences">readResult.</param>
        /// <param name="hdid">hdid.</param>
        public UserPreferenceDelegateMock(IEnumerable<UserPreference> userPreferences, string hdid)
        {
            this.Setup(s => s.GetUserPreferencesAsync(hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userPreferences);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDelegateMock"/> class.
        /// </summary>
        /// <param name="readResult">read result.</param>
        /// <param name="action">action.</param>
        public UserPreferenceDelegateMock(DbResult<UserPreference> readResult, string action)
        {
            if (action == TestConstants.UpdateAction)
            {
                this.Setup(s => s.UpdateUserPreferenceAsync(It.IsAny<UserPreference>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(readResult);
            }

            if (action == TestConstants.CreateAction)
            {
                this.Setup(s => s.CreateUserPreferenceAsync(It.IsAny<UserPreference>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(readResult);
            }
        }
    }
}
