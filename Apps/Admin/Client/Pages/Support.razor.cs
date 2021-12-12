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
    using HealthGateway.Admin.Common.Constants;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Backing logic for the Support page.
    /// </summary>
    public partial class Support : ComponentBase
    {
        private static List<UserQueryType> QueryTypes => new() { UserQueryType.PHN, UserQueryType.Email, UserQueryType.SMS, UserQueryType.HDID };

        private UserQueryType SelectedQueryType { get; set; } = UserQueryType.PHN;

        private string QueryParameter { get; set; } = string.Empty;

        private void Search()
        {
            this.Facade.LoadMessagingVerification((int)this.SelectedQueryType, this.QueryParameter);
        }
    }
}
