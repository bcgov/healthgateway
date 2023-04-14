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
namespace HealthGateway.WebClient.Server.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using HealthGateway.Database.Models;

    /// <summary>
    /// The Tour Configuration.
    /// </summary>
    public record TourConfiguration
    {
        /// <summary>
        /// Gets or sets the tour update date time.
        /// </summary>
        public DateTime? LatestChangeDateTime { get; set; }
    }

#pragma warning disable SA1600
#pragma warning disable SA1602
    internal static class TourSettingsMapper
    {
        public const string Application = "WEB";
        public const string Component = "Tour";
        public const string LatestChangeDateTime = "latestChangeDateTime";

        public static TourConfiguration Map(IList<ApplicationSetting> settings)
        {
            TourConfiguration config = new();
            foreach (ApplicationSetting setting in settings)
            {
                switch (setting.Key)
                {
                    case LatestChangeDateTime:
                        config.LatestChangeDateTime = setting.Value != null
                            ? DateTime.Parse(setting.Value, CultureInfo.InvariantCulture)
                            : null;
                        break;
                }
            }

            return config;
        }
    }
#pragma warning restore SA1600
#pragma warning restore SA1602
}
