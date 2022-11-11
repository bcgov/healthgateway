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
        private ServiceType(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the Database service code.
        /// </summary>
        public static ServiceType Database => new("DB");

        /// <summary>
        /// Gets the Client Registries service code.
        /// </summary>
        public static ServiceType ClientRegistries => new("CR");

        /// <summary>
        /// Gets the ODRRecords service code.
        /// </summary>
        public static ServiceType OdrRecords => new("ODR");

        /// <summary>
        /// Gets the Medication service code.
        /// </summary>
        public static ServiceType Medication => new("MED");

        /// <summary>
        /// Gets the Laboratory service code.
        /// </summary>
        public static ServiceType Laboratory => new("LAB");

        /// <summary>
        /// Gets the Immunization service code.
        /// </summary>
        public static ServiceType Immunization => new("IMZ");

        /// <summary>
        /// Gets the Patient service code.
        /// </summary>
        public static ServiceType Patient => new("PAT");

        /// <summary>
        /// Gets the PHSA service code.
        /// </summary>
        public static ServiceType Phsa => new("PHSA");

        /// <summary>
        /// Gets the Salesforce service code.
        /// </summary>
        public static ServiceType Sf => new("SF");

        /// <summary>
        /// Gets the CDogs service code.
        /// </summary>
        public static ServiceType CDogs => new("CDOGS");

        /// <summary>
        /// Gets the SFTP service code.
        /// </summary>
        public static ServiceType Sftp => new("SFTP");

        /// <summary>
        /// Gets the BCMP service code.
        /// </summary>
        public static ServiceType Bcmp => new("BCMP");

        /// <summary>
        /// Gets the Keycloak service code.
        /// </summary>
        public static ServiceType Keycloak => new("KEYCLOAK");

        /// <summary>
        /// Gets or sets the Value that holds the internal representation of the ServiceType.
        /// </summary>
        public string Value { get; set; }
    }
}
