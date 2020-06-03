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
    public class TermsOfServiceModel
    {
        /// <summary>
        /// Gets or sets the terms of service id.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the terms of service html.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the date the terms of service becomes active.
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Constructs a TermsOfService model from a LegalAgreement database model.
        /// </summary>
        /// <param name="model">The legal agreement database model.</param>
        /// <returns>The terms of service model.</returns>
        public static TermsOfServiceModel CreateFromDbModel(LegalAgreement model)
        {
            return new TermsOfServiceModel()
            {
                Id = model?.Id,
                EffectiveDate = model?.EffectiveDate!.Value,
                Content = model?.LegalText,
            };
        }
    }
}