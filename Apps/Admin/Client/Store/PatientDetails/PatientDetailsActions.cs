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
namespace HealthGateway.Admin.Client.Store.PatientDetails
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class PatientDetailsActions
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
        public class LoadSuccessAction : BaseSuccessAction<PatientSupportDetails>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            /// <param name="hdid">hdid associated with the data.</param>
            public LoadSuccessAction(PatientSupportDetails data, string hdid)
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

        /// <summary>
        /// The action representing the configuring of a patient's level of access.
        /// </summary>
        public class BlockAccessAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlockAccessAction"/> class.
            /// </summary>
            /// <param name="hdid">The patient's HDID to configure access for.</param>
            /// <param name="reason">The agent's reason for the access change.</param>
            /// <param name="dataSources">The list of Dataset names that will be affected, empty will grant full access.</param>
            public BlockAccessAction(string hdid, string reason, IEnumerable<DataSource> dataSources)
            {
                this.Reason = reason;
                this.DataSources = dataSources;
                this.Hdid = hdid;
            }

            /// <summary>
            /// Gets the patient's HDID to configure access for.
            /// </summary>
            public string Hdid { get; init; }

            /// <summary>
            /// Gets the list of data sources to block.
            /// </summary>
            public IEnumerable<DataSource> DataSources { get; init; }

            /// <summary>
            /// Gets the reason to block data source(s).
            /// </summary>
            public string Reason { get; init; }
        }

        /// <summary>
        /// The action representing a successful update of a patient's blocked access.
        /// </summary>
        public class BlockAccessSuccessAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlockAccessSuccessAction"/> class.
            /// </summary>
            /// <param name="hdid">Patient's HDID.</param>
            public BlockAccessSuccessAction(string hdid)
            {
                this.Hdid = hdid;
            }

            /// <summary>
            /// Gets the patient's HDID which had access configured.
            /// </summary>
            public string Hdid { get; init; }
        }

        /// <summary>
        /// The action representing a failed update of a patient's blocked access.
        /// </summary>
        public class BlockAccessFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlockAccessFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public BlockAccessFailureAction(RequestError error)
                : base(error)
            {
            }
        }
    }
}
