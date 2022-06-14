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
namespace HealthGateway.Admin.Client.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.MessageVerification;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class Support : FluxorComponent
    {
        private static List<UserQueryType> QueryTypes => new() { UserQueryType.PHN, UserQueryType.Email, UserQueryType.SMS, UserQueryType.HDID };

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<MessageVerificationState> MessageVerificationState { get; set; } = default!;

        private UserQueryType SelectedQueryType { get; set; } = UserQueryType.PHN;

        private string QueryParameter { get; set; } = string.Empty;

        private MudForm Form { get; set; } = default!;

        private bool MessagingVerificationsLoading => this.MessageVerificationState.Value.IsLoading;

        private bool MessagingVerificationsLoaded => this.MessageVerificationState.Value.Loaded;

        private bool HasError => this.MessageVerificationState.Value.Error != null && this.MessageVerificationState.Value.Error.Message.Length > 0;

        private IEnumerable<MessagingVerificationModel> MessagingVerifications =>
            this.MessageVerificationState.Value.Result?.ResourcePayload ?? Enumerable.Empty<MessagingVerificationModel>();

        private IEnumerable<MessagingVerificationRow> MessagingVerificationRows => this.MessagingVerifications.Select(v => new MessagingVerificationRow(v));

        private Func<string, string?> ValidateQueryParameter => (parameter) =>
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "Search parameter is required";
            }

            if (this.SelectedQueryType == UserQueryType.PHN && !PhnValidator.IsValid(StringManipulator.StripWhitespace(parameter)))
            {
                return "Invalid PHN";
            }

            return null;
        };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetState();
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        private void ResetState()
        {
            this.Dispatcher.Dispatch(new MessageVerificationActions.ResetStateAction());
        }

        private async Task SearchAsync()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (this.Form.IsValid)
            {
                this.ResetState();
                this.Dispatcher.Dispatch(new MessageVerificationActions.LoadAction(this.SelectedQueryType, StringManipulator.StripWhitespace(this.QueryParameter)));
            }
        }

        private sealed record MessagingVerificationRow
        {
            public MessagingVerificationRow(MessagingVerificationModel model)
            {
                this.Hdid = model.UserProfileId ?? string.Empty;
                this.PersonalHealthNumber = model.PersonalHealthNumber ?? string.Empty;
                this.EmailOrSms = model.VerificationType switch
                {
                    MessagingVerificationType.Email => model.Email?.To ?? "N/A",
                    _ => model.SMSNumber ?? "N/A",

                };
                this.Verified = model.Validated ? "true" : "false";
                this.VerificationDate = DateFormatter.ToShortDateAndTime(model.UpdatedDateTime);
                this.VerificationCode = model.VerificationType switch
                {
                    MessagingVerificationType.Email => "-",
                    _ => model.SMSValidationCode ?? "-",
                };
            }

            public string Hdid { get; init; }

            public string PersonalHealthNumber { get; init; }

            public string EmailOrSms { get; init; }

            public string Verified { get; init; }

            public string VerificationDate { get; init; }

            public string VerificationCode { get; init; }
        }
    }
}
