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
namespace HealthGateway.Database.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;

    /// <inheritdoc/>
    public class DbDelegateInvitationDelegate : IDelegateInvitationDelegate
    {
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDelegateInvitationDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DbDelegateInvitationDelegate(GatewayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task UpdateDelegateInvitationAsync(DelegateInvitation delegateInvitation, bool commit = true)
        {
            if (delegateInvitation.Version == 0)
            {
                this.dbContext.DelegateInvitation.Add(delegateInvitation);
            }
            else
            {
                this.dbContext.DelegateInvitation.Update(delegateInvitation);
            }

            if (commit)
            {
                await this.dbContext.SaveChangesAsync();
            }
        }
    }
}
