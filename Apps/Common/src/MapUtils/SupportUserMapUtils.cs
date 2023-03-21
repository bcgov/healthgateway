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
namespace HealthGateway.Common.MapUtils
{
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class SupportUserMapUtils
    {
        /// <summary>
        /// Creates a UI model from a DB model.
        /// </summary>
        /// <param name="userProfile">The DB model to convert.</param>
        /// <param name="patientModel">The patient model to convert.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <returns>The created UI model.</returns>
        public static SupportUser ToUiModel(UserProfile userProfile, PatientModel patientModel, IMapper mapper)
        {
            SupportUser supportUser = mapper.Map<UserProfile, SupportUser>(
                userProfile,
                opts => opts.AfterMap(
                    (_, dest) =>
                    {
                        dest.PersonalHealthNumber = patientModel.PersonalHealthNumber;
                        dest.PhysicalAddress = AddressUtility.GetAddressAsSingleLine(patientModel.PhysicalAddress);
                        dest.PostalAddress = AddressUtility.GetAddressAsSingleLine(patientModel.PostalAddress);
                    }));
            return supportUser;
        }
    }
}
