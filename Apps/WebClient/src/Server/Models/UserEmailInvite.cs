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
    /// Model that provides a user representation of an EmailInvite.
    /// </summary>
    public class UserEmailInvite
    {
        /// <summary>
        /// Gets or sets the email invite id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the users directed identifier.
        /// </summary>
        public string? HdId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the invite was validated.
        /// </summary>
        public bool Validated { get; set; }

        /// <summary>
        /// Gets or sets the associated email that was sent for this invite.
        /// </summary>
        public Guid EmailId { get; set; }

        /// <summary>
        /// Gets or sets the associated email that was sent for this invite.
        /// </summary>

        /// <summary>
        /// Gets or sets the email address for the invite.
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Constructs a UserInviteEmail from a EmailInvite.
        /// </summary>
        /// <param name="model">The model object to convert.</param>
        /// <returns>The created view model.</returns>
        public static UserEmailInvite CreateFromDbModel(MessagingVerification model)
        {
            return new UserEmailInvite()
            {
                Id = model.Id,
                HdId = model.HdId,
                Validated = model.Validated,
                EmailId = model.EmailId!.Value,
                EmailAddress = model?.Email?.To,
            };
        }
    }
}
