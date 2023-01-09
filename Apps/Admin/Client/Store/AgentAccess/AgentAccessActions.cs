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
namespace HealthGateway.Admin.Client.Store.AgentAccess;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class AgentAccessActions
{
    /// <summary>
    /// The action representing the initiation of an add.
    /// </summary>
    public class AddAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddAction"/> class.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        public AddAction(AdminAgent agent)
        {
            this.Agent = agent;
        }

        /// <summary>
        /// Gets or sets the agent.
        /// </summary>
        public AdminAgent Agent { get; set; }
    }

    /// <summary>
    /// The action representing a failed add.
    /// </summary>
    public class AddFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public AddFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful add.
    /// </summary>
    public class AddSuccessAction : BaseSuccessAction<AdminAgent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddSuccessAction"/> class.
        /// </summary>
        /// <param name="data">Agent data.</param>
        public AddSuccessAction(AdminAgent data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of a search.
    /// </summary>
    public class SearchAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchAction"/> class.
        /// </summary>
        /// <param name="query">The query string to match agents against.</param>
        public SearchAction(string query)
        {
            this.Query = query;
        }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        public string Query { get; set; }
    }

    /// <summary>
    /// The action representing a failed search.
    /// </summary>
    public class SearchFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public SearchFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful search.
    /// </summary>
    public class SearchSuccessAction : BaseSuccessAction<IEnumerable<AdminAgent>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchSuccessAction"/> class.
        /// </summary>
        /// <param name="data">Agent data.</param>
        public SearchSuccessAction(IEnumerable<AdminAgent> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of an update.
    /// </summary>
    public class UpdateAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAction"/> class.
        /// </summary>
        /// <param name="agent">The agent model.</param>
        public UpdateAction(AdminAgent agent)
        {
            this.Agent = agent;
        }

        /// <summary>
        /// Gets or sets the agent.
        /// </summary>
        public AdminAgent Agent { get; set; }
    }

    /// <summary>
    /// The action representing a failed update.
    /// </summary>
    public class UpdateFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public UpdateFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful update.
    /// </summary>
    public class UpdateSuccessAction : BaseSuccessAction<AdminAgent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSuccessAction"/> class.
        /// </summary>
        /// <param name="data">Agent data.</param>
        public UpdateSuccessAction(AdminAgent data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of a deletion.
    /// </summary>
    public class DeleteAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAction"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the agent.</param>
        public DeleteAction(Guid id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the agent.
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// The action representing a failed deletion.
    /// </summary>
    public class DeleteFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public DeleteFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action representing a successful deletion.
    /// </summary>
    public class DeleteSuccessAction : BaseSuccessAction<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteSuccessAction"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the agent.</param>
        public DeleteSuccessAction(Guid id)
            : base(id)
        {
        }
    }

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public class ResetStateAction
    {
    }
}
