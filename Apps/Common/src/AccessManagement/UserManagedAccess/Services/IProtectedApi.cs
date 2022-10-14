//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models;
    using Refit;


    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    [Headers("Authorization: Bearer")]
    public interface IProtectedResource
    {
        /// <summary>
        /// Creates a new Resource on the authorization server. See <see cref="Resource"/> class.
        /// </summary>
        /// <param name="resource">The Resource data.</param>
        /// <returns>The Resource created.</returns>
        [Post("")]
        public Task<Resource> Create([Body] Resource resource);

        /// <summary>
        /// Updates an existing Resource on the authorization server. See <see cref="Resource"/> class.
        /// </summary>
        /// <param name="resource">The Resource to be updated.</param>
        /// <returns>True when updated.</returns>
        [Put("/")]
        public Task<bool> Update([Body] Resource resource);

        /// <summary>Deletes an existing user-managed Resource from the server.</summary>
        /// <param name="resourceId">The Resource identifier.</param>
        /// <returns>True if the delete was successful.</returns>
        [Delete("?resourceId={resourceId}")]
        public Task<bool> Delete(string resourceId);

        /// <summary>
        /// Query the server for a resource given its id.
        /// </summary>
        /// <param name="resourceId">The Resource  ID to be found.</param>
        /// <returns>The Resource found.</returns>
        [Get("")]
        public Task<Resource> FindById(string resourceId);

        /// <summary>
        /// Query the server for a Resource with a given Uri.
        /// </summary>
        /// <param name="uri">The url to be found.</param>
        /// <returns>Returns a list of Resources that best matches the given Uri.</returns>
        [Get("/")]
        public Task<List<Resource>> FindByUri(Uri uri, string token);

        /// <summary>
        /// Query the server for a Resource with a given Uri.
        /// This method queries the server for resources that match the Uri.
        /// </summary>
        /// <param name="uri">The url to be found.</param>
        /// <returns>Returns a list of Resources that best matches the given Uri.</returns>
        [Get("/")]
        public Task<List<Resource>> FindByMatchingUri(Uri uri);

        /// <summary>Query the server for all resources.</summary>
        /// <returns> @return an array of strings with the resource ids.</returns>
        [Get("/")]
        public Task<string[]> FindAll();
    }
}