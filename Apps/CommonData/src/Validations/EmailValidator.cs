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
    using System.Text.RegularExpressions;
    using FluentValidation;

    /// <summary>
    /// Validates email string.
    /// </summary>
    public partial class EmailValidator : AbstractValidator<string?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailValidator"/> class.
        /// </summary>
        public EmailValidator()
        {
            this.RuleFor(v => v).NotEmpty().Must(IsValidEmail).WithMessage("Email address is not valid");
        }

        /// <summary>
        /// Validates the supplied value is a valid email address.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValid(string? email)
        {
            return new EmailValidator().Validate(email).IsValid;
        }

        private static bool IsValidEmail(string? email)
        {
            return EmailRegex().IsMatch(email);
        }

        // The pattern checks for basic email format rules such as having characters before and after the "@" symbol,
        // a dot followed by at least two letters after the last "@" symbol, etc.
        [GeneratedRegex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$")]
        private static partial Regex EmailRegex();
    }
}
