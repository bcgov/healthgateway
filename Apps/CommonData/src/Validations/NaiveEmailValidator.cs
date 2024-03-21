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

namespace HealthGateway.Common.Data.Validations
{
    using FluentValidation;

    /// <summary>
    /// Validates email string contains an '@' symbol.
    /// </summary>
    public class NaiveEmailValidator : AbstractValidator<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaiveEmailValidator"/> class.
        /// </summary>
        /// <param name="messageOverride">A custom error message on failure.</param>
        public NaiveEmailValidator(string? messageOverride = null)
        {
            this.RuleFor(v => v).NotEmpty().EmailAddress().WithMessage(messageOverride ?? "Email is not valid");
        }

        /// <summary>
        /// Validates the supplied value is ostensibly an email address (i.e. contains an '@' symbol).
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValid(string? email)
        {
            return email != null && new NaiveEmailValidator().Validate(email).IsValid;
        }
    }
}
