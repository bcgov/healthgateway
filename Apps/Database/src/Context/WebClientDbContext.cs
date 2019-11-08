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
namespace HealthGateway.Database.Context
{
    using HealthGateway.Database.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The database context used by the web client application.
    /// </summary>
    public class WebClientDbContext : BaseDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebClientDbContext"/> class.
        /// </summary>
        /// <param name="options">The DB Context options.</param>
        public WebClientDbContext(DbContextOptions<WebClientDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the set of Audit Events.
        /// </summary>
        public DbSet<UserProfile> UserProfile { get; set; }
    }
}
