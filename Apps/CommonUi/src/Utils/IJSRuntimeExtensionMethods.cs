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
namespace HealthGateway.Common.Ui.Utils
{
    using System.Threading.Tasks;
    using Microsoft.JSInterop;

    /// <summary>
    /// Extension methods for the <see cref="IJSRuntime"/> class.
    /// </summary>
    public static class IJSRuntimeExtensionMethods
    {
        /// <summary>
        /// Calls the initializeActivityTimer() JavaScript function via JavaScript interop.
        /// </summary>
        /// <typeparam name="T">The type of the dotnet object that will be passed to the JavaScript function.</typeparam>
        /// <param name="js">The instance of the <see cref="IJSRuntime"/> class.</param>
        /// <param name="dotNetObjectReference">A reference to the instance of the dotnet object to pass to the JavaScript function.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public static async ValueTask InitializeInactivityTimer<T>(this IJSRuntime js, DotNetObjectReference<T> dotNetObjectReference)
            where T : class
        {
            await js.InvokeVoidAsync("initializeInactivityTimer", dotNetObjectReference).ConfigureAwait(true);
        }
    }
}
