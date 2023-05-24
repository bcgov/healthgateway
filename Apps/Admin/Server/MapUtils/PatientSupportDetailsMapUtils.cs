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
namespace HealthGateway.Admin.Server.MapUtils
{
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class PatientSupportDetailsMapUtils
    {
        /// <summary>
        /// Creates a UI model from a user profile model and a patient model.
        /// </summary>
        /// <param name="userProfile">The user profile model to convert.</param>
        /// <param name="patientModel">The patient model to convert.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <returns>The created UI model.</returns>
        public static PatientSupportResult ToUiModel(UserProfile? userProfile, PatientModel? patientModel, IMapper mapper)
        {
            return mapper.Map<PatientModel?, PatientSupportResult>(
                patientModel,
                opts => opts.AfterMap(
                    (_, dest) =>
                    {
                        dest.Status = PatientStatus.Default;
                        if (patientModel == null)
                        {
                            dest.Status = PatientStatus.NotFound;
                        }
                        else if (patientModel.IsDeceased == true)
                        {
                            dest.Status = PatientStatus.Deceased;
                        }
                        else if (userProfile == null)
                        {
                            dest.Status = PatientStatus.NotUser;
                        }

                        dest.ProfileCreatedDateTime = userProfile?.CreatedDateTime;
                        dest.ProfileLastLoginDateTime = userProfile?.LastLoginDateTime;

                        if (patientModel == null && userProfile != null)
                        {
                            dest.Hdid = userProfile.HdId;
                        }
                    }));
        }
    }
}
