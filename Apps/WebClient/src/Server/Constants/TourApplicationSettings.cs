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
namespace HealthGateway.WebClient.Server.Constants
{
    /// <summary>
    /// Contains the ApplicationSettings values and mapping logic for the TourConfiguration.
    /// </summary>
    public static class TourApplicationSettings
    {
        /// <summary>
        /// The application name.
        /// </summary>
        public const string Application = "WEB";

        /// <summary>
        /// The component name.
        /// </summary>
        public const string Component = "Tour";


        /// <summary>
        /// The latest change date time key.
        /// </summary>
        public static readonly string LatestChangeDateTime = "latestChangeDateTime";
    }
}
