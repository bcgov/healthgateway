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
namespace HealthGateway.Common.Delegates.IAM.Keycloak
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    using HealthGateway.Common.Authentication;
    using HealthGateway.Common.Delegates.IAM;
    using HealthGateway.Common.Models.IAM;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;


    /// <summary>
    /// The Keycloak User admin delegate.
    /// </summary>
    public class UserDelegate : IUserDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        public UserDelegate(
            ILogger<UserDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IAuthService authService)
        {
            Task<List<UserRepresentation>> FindUser(string username)
            {

            }
            
            Task<int> DeleteUser(string userId)
            {

            }


        }

    }
}