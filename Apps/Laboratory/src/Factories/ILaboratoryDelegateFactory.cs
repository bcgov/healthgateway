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
namespace HealthGateway.Laboratory.Factories
{
    using System;
    using HealthGateway.Laboratory.Delegates;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Creates the Laboratory Delegate dynamically.
    /// </summary>
    public interface ILaboratoryDelegateFactory
    {
        /// <summary>
        /// Returns a new instance of IImunizationFhirDelegate via configuration.
        /// </summary>
        /// <returns>The IImunizationFhirDelegate.</returns>
        ILaboratoryDelegate CreateInstance();
    }
}
