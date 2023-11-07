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
namespace HealthGateway.Admin.Client.Services
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.Web;

    /// <summary>
    /// Injectable service to simplify key interceptor subscriptions.
    /// </summary>
    public interface IKeyInterceptorService : IDisposable
    {
        /// <summary>
        /// Value for the enter key used by the key property in the JavaScript KeyboardEvent.
        /// </summary>
        const string EnterKey = "Enter";

        /// <summary>
        /// Registers an event handler to monitor keypresses when specific elements are focused and perform a callback action when
        /// a particular key is pressed.
        /// </summary>
        /// <param name="ancestorId">HTML ID matching ancestor element.</param>
        /// <param name="targetClass">HTML class matching target element(s) to monitor.</param>
        /// <param name="key">Name of key to monitor, matching JavaScript KeyboardEvent.key values.</param>
        /// <param name="callback">Action to perform when Enter key is pressed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RegisterOnKeyDownAsync(string ancestorId, string targetClass, string key, Func<KeyboardEventArgs, Task> callback);
    }
}
