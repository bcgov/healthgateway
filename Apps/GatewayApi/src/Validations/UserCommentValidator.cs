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

namespace HealthGateway.GatewayApi.Validations
{
    using FluentValidation;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Validates <see cref="UserComment"/> instances.
    /// </summary>
    public class UserCommentValidator : AbstractValidator<UserComment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCommentValidator"/> class.
        /// </summary>
        public UserCommentValidator()
        {
            this.RuleFor(v => v.UserProfileId).NotEmpty();
            this.RuleFor(v => v.Text).MaximumLength(1000);
        }
    }
}
