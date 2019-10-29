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
namespace HealthGateway.Medication.Services
{
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Delegate;
    using HealthGateway.Medication.Models;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestPharmacyService : IPharmacyService
    {
        /// <summary>
        /// The http context provider.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Delegate to interact with hnclient.
        /// </summary>
        private readonly IHNClientDelegate hnClientDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPharmacyService"/> class.
        /// </summary>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="hnClientDelegate">Injected HNClient Delegate.</param>
        public RestPharmacyService(IHttpContextAccessor httpAccessor, IHNClientDelegate hnClientDelegate)
        {
            this.httpContextAccessor = httpAccessor;
            this.hnClientDelegate = hnClientDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId)
        {
            string jwtString = this.httpContextAccessor.HttpContext.Request.Headers["Authorization"][0];
            string userId = this.httpContextAccessor.HttpContext.User.Identity.Name;
            IPAddress address = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            string ipv4Address = address.MapToIPv4().ToString();

            return await hnClientDelegate.GetPharmacyAsync(pharmacyId, userId, ipv4Address);
        }
    }
}