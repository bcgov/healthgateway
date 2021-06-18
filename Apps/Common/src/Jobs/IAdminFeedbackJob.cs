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
    using HealthGateway.Common.Models;

    /// <summary>
    /// A Job to send/retry sending an email to the administrator.
    /// </summary>
    public interface IAdminFeedbackJob
    {
        /// <summary>
        /// Sends an email to the Health Gateway Admin with the feedback included.
        /// </summary>
        /// <param name="clientFeedback">The client feedback to email to the admin.</param>
        void SendEmail(ClientFeedback clientFeedback);
    }
}
