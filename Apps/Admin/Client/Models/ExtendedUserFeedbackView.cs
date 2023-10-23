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
namespace HealthGateway.Admin.Client.Models
{
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// A user feedback view model with additional state information.
    /// </summary>
    public class ExtendedUserFeedbackView : UserFeedbackView
    {
        /// <summary>
        /// Gets or sets a value indicating whether the feedback has been modified or not.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Returns a shallow copy of the object.
        /// </summary>
        /// <returns>A new object containing the same values as the current object.</returns>
        public ExtendedUserFeedbackView ShallowCopy()
        {
            return (ExtendedUserFeedbackView)this.MemberwiseClone();
        }
    }
}
