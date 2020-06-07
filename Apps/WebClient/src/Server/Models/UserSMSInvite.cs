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
namespace HealthGateway.WebClient.Models
{
    using System;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of a sms verification invite.
    /// </summary>
    public class UserSMSInvite
    {
        /// <summary>
        /// Gets or sets a value indicating whether the invite was validated.
        /// </summary>
        public bool Validated { get; set; }

        /// <summary>
        /// Gets or sets the sms number for the invite.
        /// </summary>
        public string? SMSNumber { get; set; }

        /// <summary>
        /// Constructs a UserSMSInvite from a MessagingVerification.
        /// </summary>
        /// <returns>The UserSMSInvite model.</returns>
        public static UserSMSInvite CreateFromDbModel(MessagingVerification model)
        {
            return new UserSMSInvite()
            {
                Validated = model.Validated,
                SMSNumber = model.SMSNumber,
            };
        }
    }
}
