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
namespace HealthGateway.Common.Delegates.PHSA
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Interface that defines a delegate to swap access tokens with PHSA.
    /// </summary>
    public interface ITokenSwapDelegate
    {
        /// <summary>
        /// Gets an access token from PHSA with an authenticated Health Gateway access token.
        /// </summary>
        /// <param name="accessToken">The access token to swap with PHSA.</param>
        /// <returns>The newly swapped access token wrapped in a token swap response object.</returns>
        Task<RequestResult<TokenSwapResponse>> SwapToken(string accessToken);
    }
}
