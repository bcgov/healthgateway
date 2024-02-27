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
namespace HealthGateway.Patient.Services
{
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Models;
    using HealthGateway.PatientDataAccess;

    /// <inheritdoc/>
    public class PatientMappingService(IMapper mapper) : IPatientMappingService
    {
        /// <inheritdoc/>
        public DataSource MapToDataSource(PatientDataType source)
        {
            return mapper.Map<PatientDataType, DataSource>(source);
        }

        /// <inheritdoc/>
        public HealthCategory MapToHealthCategory(PatientDataType source)
        {
            return mapper.Map<PatientDataType, HealthCategory>(source);
        }

        /// <inheritdoc/>
        public PatientData MapToPatientData(HealthData source)
        {
            return mapper.Map<HealthData, PatientData>(source);
        }

        /// <inheritdoc/>
        public PatientDetails MapToPatientDetails(PatientModel source)
        {
            return mapper.Map<PatientModel, PatientDetails>(source);
        }
    }
}
