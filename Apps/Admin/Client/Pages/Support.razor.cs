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
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using Fluxor.Blazor.Web.Components;
    using HealthGateway.Admin.Client.Store.MessageVerification;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class Support : FluxorComponent
    {
        private static List<UserQueryType> QueryTypes => new() { UserQueryType.PHN, UserQueryType.Email, UserQueryType.SMS, UserQueryType.HDID };

        private UserQueryType SelectedQueryType { get; set; } = UserQueryType.PHN;

        private string QueryParameter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets dispatcher.
        /// </summary>
        [Inject]
        private IDispatcher Dispatcher { get; set; } = default!;

        private void Search()
        {
            this.Dispatcher.Dispatch(new Actions.LoadAction(this.SelectedQueryType, this.QueryParameter.Trim()));
        }

        private IEnumerable<MessagingVerificationModel>? MessagingVerificationsList()
        {
            if (this.RequestResultState?.Value?.RequestResult?.ResourcePayload != null)
            {
                return this.RequestResultState?.Value?.RequestResult?.ResourcePayload;
            }

            return Enumerable.Empty<MessagingVerificationModel>();
        }
    }
}
