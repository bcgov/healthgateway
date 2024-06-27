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
    using System.Diagnostics.CodeAnalysis;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Common.Constants;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the COVID-19 treatment assessment options for a question.
    /// </summary>
    public partial class AssessmentOptionRadio : FluxorComponent
    {
        private CovidTherapyAssessmentOption selectedOption = CovidTherapyAssessmentOption.Unspecified;

        /// <summary>
        /// Gets or sets the selected assessment option.
        /// </summary>
        [Parameter]
        [EditorRequired]
        [SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "Two-way binding")]
        public CovidTherapyAssessmentOption SelectedOption
        {
            get => this.selectedOption;

            set
            {
                if (this.selectedOption == value)
                {
                    return;
                }

                this.selectedOption = value;
                this.SelectedOptionChanged.InvokeAsync(value);
            }
        }

        /// <summary>
        /// Gets or sets the event callback that's triggered when <see cref="SelectedOption"/> changes.
        /// </summary>
        [Parameter]
        public EventCallback<CovidTherapyAssessmentOption> SelectedOptionChanged { get; set; }

        /// <summary>
        /// Gets or sets the assessment option that causes a message to display that suggests the citizen would benefit from
        /// COVID-19 treatment.
        /// </summary>
        [Parameter]
        public CovidTherapyAssessmentOption? OptionThatIndicatesBenefit { get; set; }

        /// <summary>
        /// Gets or sets the assessment option that causes a message to display that suggests the citizen would not benefit from
        /// COVID-19 treatment.
        /// </summary>
        [Parameter]
        public CovidTherapyAssessmentOption? OptionThatIndicatesNoBenefit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Not Sure" option should be included.
        /// </summary>
        [Parameter]
        public bool IncludeNotSureOption { get; set; }

        /// <summary>
        /// Gets or sets all attributes added to the component that don't match any of its parameters.
        /// They will be splatted onto the underlying element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object?> UnmatchedAttributes { get; set; } = new Dictionary<string, object?>();
    }
}
