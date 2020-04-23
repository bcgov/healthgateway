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
namespace HealthGateway.Medication.Models
{
    /// <summary>
    /// The configuration for the ODR portion of the ResetMedStatement delegate
    /// </summary>
    public class ODRConfig
    {
        public string ServiceName { get; set; }

        public string ServiceHostSuffix { get; set; } = "_HOST";

        public string ServicePortSuffix { get; set; } = "_PORT";

        public string Url { get; set; }

        public string PatientProfileEndpoint { get; set; }

        public string ProtectiveWordEndpoint { get; set; }

        public int CacheTTL { get; set; } = 1440;

        public bool DynamicServiceLookup { get; set; }
    }
}
