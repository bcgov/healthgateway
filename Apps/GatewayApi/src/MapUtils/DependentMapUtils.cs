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

namespace HealthGateway.GatewayApi.MapUtils
{
    using AutoMapper;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class DependentMapUtils
    {
        /// <summary>
        /// Converts a ResourceDelegate and PatientModel to a DependentModel.
        /// </summary>
        /// <param name="resourceDelegate">The DB model for the resource delegate.</param>
        /// <param name="patientModel">The DB model for the dependent's patient information.</param>
        /// <param name="totalDelegateCount">The total number of delegates with access to the dependent.</param>
        /// <param name="autoMapper">The automapper to use.</param>
        /// <returns>A DependentModel.</returns>
        public static DependentModel CreateFromDbModels(ResourceDelegate resourceDelegate, PatientModel patientModel, int totalDelegateCount, IMapper autoMapper)
        {
            DependentModel dependent = autoMapper.Map<ResourceDelegate, DependentModel>(resourceDelegate);
            dependent.DependentInformation = autoMapper.Map<PatientModel, DependentInformation>(patientModel);
            dependent.TotalDelegateCount = totalDelegateCount;

            return dependent;
        }
    }
}
