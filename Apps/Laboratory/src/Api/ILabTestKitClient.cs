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
namespace HealthGateway.Laboratory.Api
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Laboratory.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface that defines a client api to register lab tests.
    /// </summary>
    public interface ILabTestKitClient
    {
        /// <summary>
        /// Registers a lab test kit for a public user.
        /// </summary>
        /// <param name="testKit">The lab test kit to register.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The lab test kit sent.</returns>
        [Post("/api/v1/Public/LabTestKits/Registration")]
        Task<HttpResponseMessage> RegisterLabTest([Body] PublicLabTestKit testKit, [Authorize] string token);

        /// <summary>
        /// Registers a lab test kit for an authenticated user.
        /// </summary>
        /// <param name="hdid">The authenticated users health identifier.</param>
        /// <param name="testKit">The lab test kit to register.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>The lab test kit sent.</returns>
        [Post("/api/v1/LabTestKits/Registration?subjectHdid={HdId}")]
        Task<HttpResponseMessage> RegisterLabTest(string hdid, [Body] LabTestKit testKit, [Authorize] string token);
    }
}
