﻿// -------------------------------------------------------------------------
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
    using FluentValidation.Results;

    /// <summary>
    /// Abstract validator that returns success instead of throwing an exception when the model is null.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    public abstract class AbstractNullableValidator<T> : AbstractValidator<T?>
    {
        /// <inheritdoc/>
        protected override bool PreValidate(ValidationContext<T?> context, ValidationResult result)
        {
            return context.InstanceToValidate != null;
        }
    }
}
