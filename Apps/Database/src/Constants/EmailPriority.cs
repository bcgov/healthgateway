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
namespace HealthGateway.Database.Constants
{
    /// <summary>
    /// Represents the priority when sending emails.
    /// These values represent priorities that the application will use.
    /// The email priority is stored in the model and the DB as an integer
    /// without any constraints.  When writing batch jobs or other items ranges
    /// should be utilized to ensure that non-compliant priorities are processed.
    /// </summary>
    public static class EmailPriority
    {
#pragma warning disable CS1591, SA1600
        public const int Low = 1;
        public const int Standard = 10;
        public const int High = 100;
        public const int Urgent = 1000;
    }
}
