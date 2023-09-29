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
    public record AddAction
    {
        /// <summary>
        /// Gets the agent.
        /// </summary>
        public required AdminAgent Agent { get; init; }
    }

    /// <summary>
    /// The action representing a failed add.
    /// </summary>
    public record AddFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful add.
    /// </summary>
    public record AddSuccessAction : BaseSuccessAction<AdminAgent>;

    /// <summary>
    /// The action representing the initiation of a search.
    /// </summary>
    public record SearchAction
    {
        /// <summary>
        /// Gets the query string to match agents against.
        /// </summary>
        public required string Query { get; init; }
    }

    /// <summary>
    /// The action representing a failed search.
    /// </summary>
    public record SearchFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful search.
    /// </summary>
    public record SearchSuccessAction : BaseSuccessAction<IEnumerable<AdminAgent>>;

    /// <summary>
    /// The action representing the initiation of an update.
    /// </summary>
    public record UpdateAction
    {
        /// <summary>
        /// Gets the agent.
        /// </summary>
        public required AdminAgent Agent { get; init; }
    }

    /// <summary>
    /// The action representing a failed update.
    /// </summary>
    public record UpdateFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful update.
    /// </summary>
    public record UpdateSuccessAction : BaseSuccessAction<AdminAgent>;

    /// <summary>
    /// The action representing the initiation of a deletion.
    /// </summary>
    public record DeleteAction
    {
        /// <summary>
        /// Gets the unique identifier for the agent.
        /// </summary>
        public required Guid Id { get; init; }
    }

    /// <summary>
    /// The action representing a failed deletion.
    /// </summary>
    public record DeleteFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful deletion.
    /// </summary>
    public record DeleteSuccessAction : BaseSuccessAction<Guid>;

    /// <summary>
    /// The action that clears any error encountered during an add.
    /// </summary>
    public record ClearAddErrorAction;

    /// <summary>
    /// The action that clears any error encountered during an update.
    /// </summary>
    public record ClearUpdateErrorAction;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
