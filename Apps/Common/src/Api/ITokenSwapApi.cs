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
namespace HealthGateway.Common.Api
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using Refit;

    /// <summary>
    /// API to swap tokens with PHSA.
    /// </summary>
    public interface ITokenSwapApi
    {
        /// <summary>
        /// Api that swaps an authenticated Health Gateway access token for a PHSA access token.
        /// </summary>
        /// <param name="content">Encoded content used to swap for a PHSA access token.</param>
        /// <returns>The newly swapped access token.</returns>
        [Post("/connect/token")]
        Task<TokenSwapResponse> SwapTokenAsync([Body(BodySerializationMethod.UrlEncoded)] FormUrlEncodedContent content);
    }
}
