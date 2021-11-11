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
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Moq;

    /// <summary>
    /// MessagingVerificationDelegateMock.
    /// </summary>
    public class MessagingVerificationDelegateMock : Mock<IMessagingVerificationDelegate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingVerificationDelegateMock"/> class.
        /// </summary>
        public MessagingVerificationDelegateMock()
        {
           this.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(new MessagingVerification());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingVerificationDelegateMock"/> class.
        /// </summary>
        /// <param name="messagingVerification">messaging verification.</param>
        public MessagingVerificationDelegateMock(MessagingVerification messagingVerification)
        {
            this.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(messagingVerification);
        }
    }
}
