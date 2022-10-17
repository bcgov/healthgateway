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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class MessageVerificationActions
    {
        /// <summary>
        /// The action representing the initiation of a load.
        /// </summary>
        public class LoadAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadAction"/> class.
            /// </summary>
            /// <param name="hdid">Represents the query string being performed.</param>
            public LoadAction(string hdid)
            {
                this.Hdid = hdid;
            }

            /// <summary>
            /// Gets or sets query string.
            /// </summary>
            public string Hdid { get; set; }
        }

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
        public class LoadFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public LoadFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a successful load.
        /// </summary>
        public class LoadSuccessAction : BaseSuccessAction<RequestResult<IEnumerable<MessagingVerificationModel>>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            /// <param name="hdid">hdid associated with the data.</param>
            public LoadSuccessAction(RequestResult<IEnumerable<MessagingVerificationModel>> data, string hdid)
                : base(data)
            {
                this.Hdid = hdid;
            }

            /// <summary>
            /// Gets or sets query string.
            /// </summary>
            public string Hdid { get; set; }
        }

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public class ResetStateAction
        {
        }
    }
}
