//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Laboratory.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// Service for registering public and authenticated lab test kits.
    /// </summary>
    public interface ILabTestKitService
    {
        /// <summary>
        /// Adds a lab test kit to a non-authenticated user.
        /// </summary>
        /// <param name="testKit">The test kit to register.</param>
        /// <returns>Returns the original testKit wrapped in a RequestResult.</returns>
        Task<RequestResult<PublicLabTestKit>> RegisterLabTestKitAsync(PublicLabTestKit testKit);

        /// <summary>
        /// Adds a lab test kit to an authenticated user.
        /// </summary>
        /// <param name="hdid">The hdid to associate the test kit against.</param>
        /// <param name="testKit">The test kit to register.</param>
        /// <returns>Returns the original testKit wrapped in a RequestResult.</returns>
        Task<RequestResult<LabTestKit>> RegisterLabTestKitAsync(string hdid, LabTestKit testKit);
    }
}
