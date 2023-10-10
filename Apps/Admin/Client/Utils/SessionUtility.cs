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
namespace HealthGateway.Admin.Client.Utils
{
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    /// <summary>
    /// Utilities for interacting with the session.
    /// </summary>
    public static class SessionUtility
    {
        /// <summary>
        /// Session storage key for support query type.
        /// </summary>
        public const string SupportQueryType = "supportQueryType";

        /// <summary>
        /// Session storage key for support query string.
        /// </summary>
        public const string SupportQueryString = "supportQueryParameter";

        /// <summary>
        /// Sets the session storage item.
        /// </summary>
        /// <param name="jsRuntime">The javascript runtime used for the session storage.</param>
        /// <param name="key">The key to set the session storage item to.</param>
        /// <param name="value">The value to set the session item to.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task SetSessionStorageItem(IJSRuntime jsRuntime, string key, string value)
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
        }

        /// <summary>
        /// Gets the session storage item.
        /// </summary>
        /// <param name="jsRuntime">The javascript runtime used for the session storage.</param>
        /// <param name="key">The key to get the session storage item.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task<string> GetSessionStorageItem(IJSRuntime jsRuntime, string key)
        {
            return await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
        }
    }
}
