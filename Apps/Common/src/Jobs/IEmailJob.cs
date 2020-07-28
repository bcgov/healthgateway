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
namespace HealthGateway.Common.Jobs
{
    using System;

    /// <summary>
    /// A Job to send/retry sending emails.
    /// </summary>
    public interface IEmailJob
    {
        /// <summary>
        /// Sends an email immediately if Priority is standard or higher.
        /// </summary>
        /// <param name="emailId">The stored emailId to send.</param>
        void SendEmail(Guid emailId);

        /// <summary>
        /// Attempts to send low priority emails.
        /// </summary>
        void SendLowPriorityEmails();

        /// <summary>
        /// Attempts to send standard priority emails.
        /// </summary>
        void SendStandardPriorityEmails();

        /// <summary>
        /// Attempts to send high priority emails.
        /// </summary>
        void SendHighPriorityEmails();

        /// <summary>
        /// Attempts to send urgent priority emails.
        /// </summary>
        void SendUrgentPriorityEmails();
    }
}
