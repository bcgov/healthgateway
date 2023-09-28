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
namespace HealthGateway.Admin.Client.Store.Tag;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class TagActions
{
    /// <summary>
    /// The action representing the initiation of an add.
    /// </summary>
    public record AddAction
    {
        /// <summary>
        /// Gets the name of the tag to add.
        /// </summary>
        public required string TagName { get; init; }
    }

    /// <summary>
    /// The action representing a failed add.
    /// </summary>
    public record AddFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful add.
    /// </summary>
    public record AddSuccessAction : BaseSuccessAction<RequestResult<AdminTagView>>;

    /// <summary>
    /// The action representing the initiation of a load.
    /// </summary>
    public record LoadAction;

    /// <summary>
    /// The action representing a failed load.
    /// </summary>
    public record LoadFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful load.
    /// </summary>
    public record LoadSuccessAction : BaseSuccessAction<RequestResult<IEnumerable<AdminTagView>>>;

    /// <summary>
    /// The action representing the initiation of a deletion.
    /// </summary>
    public record DeleteAction
    {
        /// <summary>
        /// Gets the tag.
        /// </summary>
        public required AdminTagView AdminTagView { get; init; }
    }

    /// <summary>
    /// The action representing a failed deletion.
    /// </summary>
    public record DeleteFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful deletion.
    /// </summary>
    public record DeleteSuccessAction : BaseSuccessAction<RequestResult<AdminTagView>>;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
