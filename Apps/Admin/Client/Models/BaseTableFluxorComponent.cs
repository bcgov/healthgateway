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
    using System;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Services;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A base record for table rows.
    /// Provides common functionality for table rows. Such as DateTime conversions.
    /// </summary>
    public abstract class BaseTableFluxorComponent : FluxorComponent
    {
        [Inject]
        private IDateConversionService DateConversionService { get; set; } = default!;

        /// <summary>
        /// Converts UTC datetime values to the system configured timezone and formats to a short date and time.
        /// </summary>
        /// <param name="utcDateTime">A UTC DateTime instance.</param>
        /// <param name="fallbackString">In the event utcDateTime is null, provide a fallback string to return.</param>
        /// <returns>The short formatted date and time string.</returns>
        protected string ShortFormatFromUtc(DateTime? utcDateTime, string fallbackString = "-")
        {
            return this.DateConversionService.ConvertToShortFormatFromUtc(utcDateTime, fallbackString);
        }
    }
}
