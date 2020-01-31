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
namespace HealthGateway.Admin.Models
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of an EmailInvite.
    /// </summary>
    public class UserBetaRequest
    {
        /// <summary>
        /// Gets or sets the beta request id.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email for the beta request.
        /// </summary>
        public string EmailAddress { get; set; } = null!;

        /// <summary>
        /// Gets or sets the version of the resource.
        /// </summary>
        public uint Version { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date when the request was created.
        /// </summary>
        public DateTime RegistrationDatetime { get; set; }

        /// <summary>
        /// Constructs a UserBetaRequest from a BetaRequest model.
        /// </summary>
        /// <param name="model">A beta request models.</param>
        /// <returns>A new UserBetaRequest.</returns>
        public static UserBetaRequest CreateFromDbModel(BetaRequest model)
        {
            return new UserBetaRequest()
            {
                Id = model.HdId,
                EmailAddress = model.EmailAddress,
                Version = model.Version,
                RegistrationDatetime = model.CreatedDateTime
            };
        }

        /// <summary>
        /// Constructs a List of UserBetaRequest from a list of BetaRequest models.
        /// </summary>
        /// <param name="models">List of beta request models.</param>
        /// <returns>A list of UserBetaRequest.</returns>
        public static List<UserBetaRequest> CreateListFromDbModel(List<BetaRequest> models)
        {
            List<UserBetaRequest> newList = new List<UserBetaRequest>();
            foreach (BetaRequest model in models)
            {
                newList.Add(UserBetaRequest.CreateFromDbModel(model));
            }
            return newList;
        }
    }
}
