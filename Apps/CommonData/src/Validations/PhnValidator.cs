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
    using System;
    using System.Globalization;
    using System.Linq;
    using FluentValidation;

    /// <summary>
    /// Validates Personal Health Number string.
    /// </summary>
    public class PhnValidator : AbstractValidator<string?>
    {
        private static readonly int[] PhnSigDigits = { 2, 4, 8, 5, 10, 9, 7, 3 };

        /// <summary>
        /// Initializes a new instance of the <see cref="PhnValidator"/> class.
        /// </summary>
        public PhnValidator()
        {
            this.RuleFor(v => v).NotEmpty().Must(v => IsValidInternal(v)).WithMessage("PHN is not valid");
        }

        /// <summary>
        /// Validates the supplied value is a proper Personal Health Number.
        /// </summary>
        /// <param name="phn">The Personal Health Number to validate.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValid(string? phn) => new PhnValidator().Validate(phn).IsValid;

        private static bool IsValidInternal(string phn)
        {
            if (string.IsNullOrEmpty(phn) || phn.Length != 10 || !phn.All(c => char.IsDigit(c)) || phn[0] != '9')
            {
                return false;
            }

            int checksum = 0;
            for (int i = 1; i < 9; i++)
            {
                int digit = Convert.ToInt16(phn[i].ToString(), CultureInfo.InvariantCulture);
                checksum += (digit * PhnSigDigits[i - 1]) % 11;
            }

            checksum = 11 - (checksum % 11);
            return Convert.ToInt16(phn[9].ToString(), CultureInfo.InvariantCulture) == checksum;
        }
    }
}
