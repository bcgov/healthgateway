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
namespace HealthGateway.Admin.Client.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the messaging verification table page.
    /// </summary>
    public partial class MessageVerificationTable : FluxorComponent
    {
        /// <summary>
        /// Gets or sets the data with which to populate the table.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public IEnumerable<MessagingVerificationModel> Data { get; set; } = Enumerable.Empty<MessagingVerificationModel>();

        /// <summary>
        /// Gets or sets a value indicating whether data is loading.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public bool IsLoading { get; set; } = true;

        private IEnumerable<MessagingVerificationRow> Rows => this.Data.Select(mv => new MessagingVerificationRow(mv));

        private sealed record MessagingVerificationRow
        {
            public MessagingVerificationRow(MessagingVerificationModel model)
            {
                this.EmailOrSms = model.VerificationType switch
                {
                    MessagingVerificationType.Email => model.Email ?? "N/A",
                    _ => model.SmsNumber ?? "N/A",
                };
                this.Verified = model.Validated ? "true" : "false";
                this.VerificationDate = model.UpdatedDateTime;
                this.VerificationCode = model.VerificationType switch
                {
                    MessagingVerificationType.Email => "-",
                    _ => model.SmsValidationCode ?? "-",
                };
            }

            public string EmailOrSms { get; }

            public string Verified { get; }

            public DateTime VerificationDate { get; }

            public string VerificationCode { get; }
        }
    }
}
