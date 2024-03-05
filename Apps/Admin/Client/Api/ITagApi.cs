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
namespace HealthGateway.Admin.Client.Api;

using System.Collections.Generic;
using System.Threading.Tasks;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Models;
using Refit;

/// <summary>
/// API for interacting with tags.
/// </summary>
public interface ITagApi
{
    /// <summary>
    /// Adds a tag.
    /// </summary>
    /// <param name="tagName">The tag name.</param>
    /// <returns>The wrapped model.</returns>
    [Post("/")]
    Task<RequestResult<AdminTagView>> AddAsync([Body(BodySerializationMethod.Serialized)] string tagName);

    /// <summary>
    /// Gets all tags.
    /// </summary>
    /// <returns>The wrapped collection of models.</returns>
    [Get("/")]
    Task<RequestResult<IEnumerable<AdminTagView>>> GetAllAsync();

    /// <summary>
    /// Deletes a tag.
    /// </summary>
    /// <param name="tag">The model to delete.</param>
    /// <returns>The wrapped model.</returns>
    [Delete("/")]
    Task<RequestResult<AdminTagView>> DeleteAsync([Body] AdminTagView tag);
}
