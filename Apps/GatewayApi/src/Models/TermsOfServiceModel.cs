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
namespace HealthGateway.GatewayApi.Models
{
    using System;

    /// <summary>
    /// Model that provides Terms of service model.
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
    }
}
