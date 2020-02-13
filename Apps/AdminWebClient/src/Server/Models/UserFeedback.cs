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
namespace HealthGateway.Admin.Models
{
    using System;

    /// <summary>
    /// Model that provides a user representation of a user feedback.
    /// </summary>
    public class UserFeedback
    {
        /// <summary>
        /// Gets or sets the user feedback id.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the feedback comments.
        /// </summary>
        public string Comments { get; set; } = null!;

        /// <summary>
        /// Gets or sets the value indicating whether the user is satisfied or not.
        /// </summary>
        public bool IsSatisfied { get; set; } = false;

        /// <summary>
        /// Gets or sets the value indicating whether the feedback is reviewed or not.
        /// </summary>
        public bool IsReviewed { get; set; } = false;

        /// <summary>
        /// Gets or sets the date when the feedback was created.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }
    }
}
