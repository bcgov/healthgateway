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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using HealthGateway.Common.Database.Models;

    /// <summary>
    /// The Authorization service
    /// </summary>
    public class AuditService : IAuditService
    {
        public AuditService(IConfiguration config)
        {

        }
        public Task WriteAuditEvent(AuditEvent auditEvent)
        {
            Task t = Task.Factory.StartNew(() => 
            {
                // Execute audit logging into database context

            } );
            
            return t;
        }
    }
}