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
namespace HealthGateway.Admin.Client.Components
{
    using System.Diagnostics.CodeAnalysis;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the HgTextField component.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the field.</typeparam>
    [SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Used in .razor file")]
    public partial class HgTextField<T> : HgComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HgTextField{T}"/> class.
        /// </summary>
        public HgTextField()
        {
            this.HorizontalMarginSize = 3;
            this.VerticalMarginSize = 3;
            this.TopMargin = Breakpoint.Always;
        }
    }
}
