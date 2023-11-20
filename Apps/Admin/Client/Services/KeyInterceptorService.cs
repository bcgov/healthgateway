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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.Web;
    using MudBlazor.Services;

    /// <inheritdoc/>
    public class KeyInterceptorService : IKeyInterceptorService
    {
        private readonly List<IKeyInterceptor> keyInterceptors = [];
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyInterceptorService"/> class.
        /// </summary>
        /// <param name="keyInterceptorFactory">Injected key interceptor factory.</param>
        public KeyInterceptorService(IKeyInterceptorFactory keyInterceptorFactory)
        {
            this.KeyInterceptorFactory = keyInterceptorFactory;
        }

        private IKeyInterceptorFactory KeyInterceptorFactory { get; }

        /// <inheritdoc/>
        public async Task RegisterOnKeyDownAsync(string ancestorId, string targetClass, string key, Func<KeyboardEventArgs, Task> callback)
        {
            IKeyInterceptor keyInterceptor = this.KeyInterceptorFactory.Create();

            await keyInterceptor.Connect(
                ancestorId,
                new()
                {
                    TargetClass = targetClass,
                    Keys = [new() { Key = key, PreventDown = "key+none", SubscribeDown = true }],
                });

            keyInterceptor.KeyDown += async args => await callback(args);

            this.keyInterceptors.Add(keyInterceptor);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this class and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// If true, releases both managed and unmanaged resources. If false, releases only unmanaged
        /// resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            for (int i = this.keyInterceptors.Count - 1; i >= 0; i--)
            {
                this.keyInterceptors[i].Dispose();
            }
        }
    }
}
