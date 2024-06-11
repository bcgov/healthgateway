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
namespace HealthGateway.Admin.Client.Components.Support
{
    using System.Collections.Generic;
    using Fluxor.Blazor.Web.Components;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for a question on the COVID-19 treatment assessment.
    /// </summary>
    public partial class AssessmentQuestion : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the question number.
        /// </summary>
        [Parameter]
        public int? Number { get; set; }

        /// <summary>
        /// Gets or sets additional information that should be displayed.
        /// </summary>
        [Parameter]
        public string AdditionalInfo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the additional information should be displayed.
        /// </summary>
        [Parameter]
        public bool DisplayAdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets the child content of this component.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets all attributes added to the component that don't match any of its parameters.
        /// They will be splatted onto the underlying element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object?> UnmatchedAttributes { get; set; } = new Dictionary<string, object?>();
    }
}
