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
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Client.Store.MessageVerification;
    using HealthGateway.Admin.Client.Store.SupportUser;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using MudBlazor;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class SupportPage : FluxorComponent
    {
        private static readonly PhnValidator PhnValidator = new();

        private static List<UserQueryType> QueryTypes => new() { UserQueryType.Phn, UserQueryType.Email, UserQueryType.Sms, UserQueryType.Hdid, UserQueryType.Dependent };

        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        [Inject]
        private IState<MessageVerificationState> MessageVerificationState { get; set; } = default!;

        [Inject]
        private IState<SupportUserState> SupportUserState { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        private UserQueryType QueryType { get; set; } = UserQueryType.Phn;

        private UserQueryType SelectedQueryType
        {
            get => this.QueryType;

            set
            {
                this.ResetSupportUserState();
                this.QueryParameter = string.Empty;
                this.QueryType = value;
            }
        }

        private string QueryParameter { get; set; } = string.Empty;

        private MudForm Form { get; set; } = default!;

        private bool MessagingVerificationsLoading => this.MessageVerificationState.Value.IsLoading;

        private bool PhnOrDependentSelected => this.SelectedQueryType is UserQueryType.Phn or UserQueryType.Dependent;

        private bool HasMessagingVerificationsError => this.MessageVerificationState.Value.Error is { Message.Length: > 0 };

        private IEnumerable<MessagingVerificationModel> MessagingVerifications => this.MessageVerificationState.Value.Data ?? Enumerable.Empty<MessagingVerificationModel>();

        private bool SupportUsersLoading => this.SupportUserState.Value.IsLoading;

        private bool SupportUsersLoaded => this.SupportUserState.Value.Loaded;

        private bool HasSupportUsersError => this.SupportUserState.Value.Error is { Message.Length: > 0 };

        private bool HasSupportUsersWarning => this.SupportUserState.Value.WarningMessage is { Length: > 0 };

        private IEnumerable<ExtendedSupportUser> SupportUsers =>
            this.SupportUserState.Value.Data ?? Enumerable.Empty<ExtendedSupportUser>();

        private IEnumerable<SupportUserRow> SupportUserRows => this.SupportUsers.Select(v => new SupportUserRow(v));

        private Func<string, string?> ValidateQueryParameter => parameter =>
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return "Search parameter is required";
            }

            if (this.PhnOrDependentSelected && !PhnValidator.Validate(StringManipulator.StripWhitespace(parameter)).IsValid)
            {
                return "Invalid PHN";
            }

            if ((this.SelectedQueryType == UserQueryType.Email || this.SelectedQueryType == UserQueryType.Sms) && StringManipulator.StripWhitespace(parameter)?.Length < 5)
            {
                return "Email/SMS must be minimum 5 characters";
            }

            if (this.SelectedQueryType == UserQueryType.Sms && !StringManipulator.IsPositiveNumeric(parameter))
            {
                return "SMS must contain digits only";
            }

            return null;
        };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.ResetSupportUserState();
            this.ResetMessageVerificationState();

            Uri uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(UserQueryType.Hdid.ToString(), out StringValues hdid))
            {
                this.Dispatcher.Dispatch(new SupportUserActions.LoadAction(UserQueryType.Hdid, StringManipulator.StripWhitespace(hdid)));
                this.QueryParameter = hdid!;
                this.SelectedQueryType = UserQueryType.Hdid;
            }
        }

        private static string FormatQueryType(UserQueryType queryType)
        {
            return queryType switch
            {
                UserQueryType.Hdid => "HDID",
                UserQueryType.Phn => "PHN",
                UserQueryType.Sms => "SMS",
                _ => queryType.ToString(),
            };
        }

        private IList<MessagingVerificationModel> GetMessagingVerificationModels(string hdid)
        {
            return this.MessagingVerifications.Where(v => v.UserProfileId == hdid).ToList();
        }

        private bool HasMessagingVerification(string hdid)
        {
            return this.MessagingVerifications.ToList().Exists(v => v.UserProfileId == hdid);
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        private void ResetSupportUserState()
        {
            this.Dispatcher.Dispatch(new SupportUserActions.ResetStateAction());
        }

        /// <summary>
        /// Resets the component to its initial state.
        /// </summary>
        private void ResetMessageVerificationState()
        {
            this.Dispatcher.Dispatch(new MessageVerificationActions.ResetStateAction());
        }

        private void ToggleExpandRow(string hdid)
        {
            if (!this.HasMessagingVerification(hdid))
            {
                this.Dispatcher.Dispatch(new MessageVerificationActions.LoadAction(hdid));
            }

            this.Dispatcher.Dispatch(new SupportUserActions.ToggleIsExpandedAction(hdid));
        }

        private async Task SearchAsync()
        {
            await this.Form.Validate().ConfigureAwait(true);
            if (this.Form.IsValid)
            {
                this.ResetSupportUserState();
                this.ResetMessageVerificationState();
                this.Dispatcher.Dispatch(new SupportUserActions.LoadAction(this.SelectedQueryType, StringManipulator.StripWhitespace(this.QueryParameter)));
            }
        }

        private sealed record SupportUserRow
        {
            public SupportUserRow(ExtendedSupportUser model)
            {
                this.Hdid = model.Hdid;
                this.PersonalHealthNumber = model.PersonalHealthNumber;
                this.LastLoginDateTime = model.LastLoginDateTime;
                this.IsExpanded = model.IsExpanded;
            }

            public string Hdid { get; }

            public string PersonalHealthNumber { get; }

            public DateTime? LastLoginDateTime { get; }

            public bool IsExpanded { get; }
        }
    }
}
