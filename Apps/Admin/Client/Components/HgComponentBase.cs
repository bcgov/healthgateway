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
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Client.Constants;
    using HealthGateway.Admin.Client.Utils;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Base class containing shared functionality for Health Gateway components.
    /// </summary>
    public abstract class HgComponentBase : ComponentBase
    {
        /// <summary>
        /// Gets or sets the child content of this component.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets class names, separated by space.
        /// </summary>
        [Parameter]
        public string Class { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets user styles, applied on top of the component's own classes and styles.
        /// </summary>
        [Parameter]
        public string Style { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a data object to attach to the component for convenience.
        /// </summary>
        [Parameter]
        public object? Tag { get; set; }

        /// <summary>
        /// Gets or sets all attributes added to the component that don't match any of its parameters.
        /// They will be splatted onto the underlying element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> UnmatchedAttributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating at what breakpoint to include the standard left margin on the component.
        /// </summary>
        [Parameter]
        public Breakpoint LeftMargin { get; set; } = Breakpoint.None;

        /// <summary>
        /// Gets or sets a value indicating at what breakpoint to include the standard right margin on the component.
        /// </summary>
        [Parameter]
        public Breakpoint RightMargin { get; set; } = Breakpoint.None;

        /// <summary>
        /// Gets or sets a value indicating at what breakpoint to include the standard top margin on the component.
        /// </summary>
        [Parameter]
        public Breakpoint TopMargin { get; set; } = Breakpoint.None;

        /// <summary>
        /// Gets or sets a value indicating at what breakpoint to include the standard bottom margin on the component.
        /// </summary>
        [Parameter]
        public Breakpoint BottomMargin { get; set; } = Breakpoint.None;

        /// <summary>
        /// Gets or sets the standard size of horizontal margins on the component.
        /// </summary>
        protected uint HorizontalMarginSize { get; set; }

        /// <summary>
        /// Gets or sets the standard size of vertical margins on the component.
        /// </summary>
        protected uint VerticalMarginSize { get; set; }

        /// <summary>
        /// Gets the combined HTML class including standard margins.
        /// </summary>
        protected string CombinedClass
        {
            get
            {
                string leftMarginClass = SpacingUtility.GenerateMarginClasses(SpacingDirectionCode.Left, this.LeftMargin, this.HorizontalMarginSize);
                string rightMarginClass = SpacingUtility.GenerateMarginClasses(SpacingDirectionCode.Right, this.RightMargin, this.HorizontalMarginSize);
                string topMarginClass = SpacingUtility.GenerateMarginClasses(SpacingDirectionCode.Top, this.TopMargin, this.VerticalMarginSize);
                string bottomMarginClass = SpacingUtility.GenerateMarginClasses(SpacingDirectionCode.Bottom, this.BottomMargin, this.VerticalMarginSize);

                List<string> classes = new()
                {
                    leftMarginClass,
                    rightMarginClass,
                    topMarginClass,
                    bottomMarginClass,
                    this.Class,
                };

                return string.Join(" ", classes.Where(c => !string.IsNullOrEmpty(c)));
            }
        }
    }
}
