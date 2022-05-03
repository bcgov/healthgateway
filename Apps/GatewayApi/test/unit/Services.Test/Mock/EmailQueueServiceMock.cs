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
    using HealthGateway.Common.Services;
    using Moq;

    /// <summary>
    /// EmailQueueServiceMock.
    /// </summary>
    public class EmailQueueServiceMock : Mock<IEmailQueueService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQueueServiceMock"/> class.
        /// </summary>
        /// <param name="shouldCommit">check if allowing to commit.</param>
        public EmailQueueServiceMock(bool shouldCommit)
        {
            this.Setup(s => s.QueueNewEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), shouldCommit));
        }
    }
}
