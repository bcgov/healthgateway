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

namespace HealthGateway.Admin.Client.Store.MessageVerification
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class MessageVerificationEffects
    {
        public MessageVerificationEffects(ILogger<MessageVerificationEffects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<MessageVerificationEffects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        [EffectMethod]
        public async Task HandleLoadAction(MessageVerificationActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading messaging verifications");

            try
            {
                RequestResult<IEnumerable<MessagingVerificationModel>> response = await this.SupportApi.GetMessagingVerificationsAsync(action.Hdid).ConfigureAwait(true);
                if (response.ResultStatus == ResultType.Success)
                {
                    this.Logger.LogInformation("Messaging verifications loaded successfully!");
                    dispatcher.Dispatch(new MessageVerificationActions.LoadSuccessAction(response, action.Hdid));
                }
                else
                {
                    RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                    this.Logger.LogError("Error loading messaging verifications, reason: {ErrorMessage}", error.Message);
                    dispatcher.Dispatch(new MessageVerificationActions.LoadFailAction(error));
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading messaging verifications...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading messaging verifications, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new MessageVerificationActions.LoadFailAction(error));
            }
        }
    }
}
