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
namespace HealthGateway.Common.Constants
{
    /// <summary>
    /// A class with constants representing the various email template names.
    /// </summary>
    public static class EmailTemplateName
    {
        /// <summary>
        /// Name of the registration email template.
        /// </summary>
        public const string RegistrationTemplate = "Registration";

        /// <summary>
        /// Name of the beta confirmation email template.
        /// </summary>
        public const string BetaConfirmationTemplate = "BetaConfirmation";

        /// <summary>
        /// Name of the invite email template.
        /// </summary>
        public const string InviteTemplate = "Invite";

        /// <summary>
        /// Name of the close account email template.
        /// </summary>
        public const string AccountClosedTemplate = "AccountClosed";

        /// <summary>
        /// Name of the recover account email template.
        /// </summary>
        public const string AccountRecoveredTemplate = "AccountRecovered";
    }
}