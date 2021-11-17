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
        public static ServiceType Database
        {
            get { return new ServiceType("DB"); }
        }

        /// <summary>
        /// Gets the Client Registries service code.
        /// </summary>
        public static ServiceType ClientRegistries
        {
            get { return new ServiceType("CR"); }
        }

        /// <summary>
        /// Gets the ODRRecords service code.
        /// </summary>
        public static ServiceType ODRRecords
        {
            get { return new ServiceType("ODR"); }
        }

        /// <summary>
        /// Gets the Medication service code.
        /// </summary>
        public static ServiceType Medication
        {
            get { return new ServiceType("MED"); }
        }

        /// <summary>
        /// Gets the Laboratory service code.
        /// </summary>
        public static ServiceType Laboratory
        {
            get { return new ServiceType("LAB"); }
        }

        /// <summary>
        /// Gets the Immunization service code.
        /// </summary>
        public static ServiceType Immunization
        {
            get { return new ServiceType("IMZ"); }
        }

        /// <summary>
        /// Gets the Patient service code.
        /// </summary>
        public static ServiceType Patient
        {
            get { return new ServiceType("PAT"); }
        }

        /// <summary>
        /// Gets the PHSA service code.
        /// </summary>
        public static ServiceType PHSA
        {
            get { return new ServiceType("PHSA"); }
        }

        /// <summary>
        /// Gets the Salesforce service code.
        /// </summary>
        public static ServiceType SF
        {
            get { return new ServiceType("SF"); }
        }

        /// <summary>
        /// Gets the CDogs service code.
        /// </summary>
        public static ServiceType CDogs
        {
            get { return new ServiceType("CDOGS"); }
        }

        /// <summary>
        /// Gets the SFTP service code.
        /// </summary>
        public static ServiceType SFTP
        {
            get { return new ServiceType("SFTP"); }
        }

        /// <summary>
        /// Gets the BCMP service code.
        /// </summary>
        public static ServiceType BCMP
        {
            get { return new ServiceType("BCMP"); }
        }

        /// <summary>
        /// Gets or sets the Value that holds the internal representation of the ServiceType.
        /// </summary>
        public string Value { get; set; }
    }
}
