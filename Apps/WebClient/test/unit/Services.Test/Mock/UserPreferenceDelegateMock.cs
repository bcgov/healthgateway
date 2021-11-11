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
    using System.Collections.Generic;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Moq;

    /// <summary>
    /// UserPreferenceDelegateMock.
    /// </summary>
    public class UserPreferenceDelegateMock : Mock<IUserPreferenceDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDelegateMock"/> class.
        /// </summary>
        /// <param name="readResult">readResult.</param>
        /// <param name="hdid">hdid.</param>
        public UserPreferenceDelegateMock(DBResult<IEnumerable<UserPreference>> readResult, string hdid)
        {
            this.Setup(s => s.GetUserPreferences(hdid)).Returns(readResult);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceDelegateMock"/> class.
        /// </summary>
        /// <param name="readResult">readResult.</param>
        /// <param name="action">action.</param>
        public UserPreferenceDelegateMock(DBResult<UserPreference> readResult, string action)
        {
            if (action == "update")
            {
                this.Setup(s => s.UpdateUserPreference(It.IsAny<UserPreference>(), It.IsAny<bool>())).Returns(readResult);
            }

            if (action == "create")
            {
                this.Setup(s => s.CreateUserPreference(It.IsAny<UserPreference>(), It.IsAny<bool>())).Returns(readResult);
            }
        }
    }
}
