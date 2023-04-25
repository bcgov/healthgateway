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
namespace HealthGateway.Patient.Mappings
{
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Patient.Models;

    /// <summary>
    /// Patient data access mappings.
    /// </summary>
    public class AccountDataAccessMappings : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDataAccessMappings"/> class.
        /// </summary>
        public AccountDataAccessMappings()
        {
            this.CreateMap<PatientModel, PatientDetails>();
            this.CreateMap<Address, Common.Data.Models.Address>();
        }
    }
}
