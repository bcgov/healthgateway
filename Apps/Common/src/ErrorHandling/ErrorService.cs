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
namespace HealthGateway.Common.ErrorHandling
{
    /// <summary>
    /// Class that defines the different types of service types codified as strings.
    /// </summary>
    public class ServiceType
    {
        private ServiceType(string value) { Value = value; }

        /// <summary>
        /// Value that holds the internal representation of the ServiceType.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Database error.
        /// </summary>
        public static ServiceType Database { get { return new ServiceType("DB"); } }
        /// <summary>
        /// Communication with the Client registries service failed.
        /// </summary>
        public static ServiceType ClientRegistries { get { return new ServiceType("CR"); } }
        /// <summary>
        /// Communication with an the ODR services failed.
        /// </summary>
        public static ServiceType ODRRecords { get { return new ServiceType("ODR"); } }
    }
}