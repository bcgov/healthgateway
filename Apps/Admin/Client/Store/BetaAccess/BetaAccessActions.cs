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
namespace HealthGateway.Admin.Client.Store.BetaAccess;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class BetaAccessActions
{
    /// <summary>
    /// The action representing the initiation of a set user access.
    /// </summary>
    public record SetUserAccessAction
    {
        /// <summary>
        /// Gets the request data.
        /// </summary>
        public required UserBetaAccess Request { get; init; }
    }

    /// <summary>
    /// The action representing a failed set user access.
    /// </summary>
    public record SetUserAccessFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful set user access.
    /// </summary>
    public record SetUserAccessSuccessAction
    {
        /// <summary>
        /// Gets the request data.
        /// </summary>
        public required UserBetaAccess Request { get; init; }
    }

    /// <summary>
    /// The action representing the initiation of a get user access.
    /// </summary>
    public record GetUserAccessAction
    {
        /// <summary>
        /// Gets the email address to check.
        /// </summary>
        public required string Email { get; init; }
    }

    /// <summary>
    /// The action representing a failed get user access.
    /// </summary>
    public record GetUserAccessFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful get user access.
    /// </summary>
    public record GetUserAccessSuccessAction : BaseSuccessAction<UserBetaAccess>;

    /// <summary>
    /// The action representing the initiation of a get all user access.
    /// </summary>
    public record GetAllUserAccessAction;

    /// <summary>
    /// The action representing a failed get all user access.
    /// </summary>
    public record GetAllUserAccessFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing a successful get all user access.
    /// </summary>
    public record GetAllUserAccessSuccessAction : BaseSuccessAction<IEnumerable<UserBetaAccess>>;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
