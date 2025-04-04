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

namespace HealthGateway.CommonUi.Tests.Utils
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Ui.Utils;
    using Microsoft.JSInterop;
    using Microsoft.JSInterop.Infrastructure;
    using Moq;
    using Xunit;

    /// <summary>
    /// IJSRuntimeExtensionMethods unit tests.
    /// </summary>
    public class IjsRuntimeExtensionMethodsTests
    {
        /// <summary>
        /// Validate InitializeInactivityTimer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInitializeInactivityTimer()
        {
            Mock<IJSRuntime> jsRuntime = new();
            using DotNetObjectReference<IjsRuntimeExtensionMethodsTests> objectReference = DotNetObjectReference.Create(this);
            await jsRuntime.Object.InitializeInactivityTimer(objectReference);

            jsRuntime.Verify(
                a => a.InvokeAsync<IJSVoidResult>(
                    It.Is<string>(
                        s =>
                            s.Equals("initializeInactivityTimer", StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<object?[]?>()),
                Times.Once);
        }
    }
}
