// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Converters;

using System.Collections.Generic;
using System.Linq;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// Converts between DB and UI models.
/// </summary>
public static class UserFeedbackTagConverter
{
    /// <summary>
    /// Creates a UI model from a DB model.
    /// </summary>
    /// <param name="model">The DB model to convert.</param>
    /// <returns>The created UI model.</returns>
    public static UserFeedbackTagView ToUiModel(this Database.Models.UserFeedbackTag model)
    {
        UserFeedbackTagView retVal = new()
        {
            Id = model.UserFeedbackTagId,
            Version = model.Version,
            TagId = model.AdminTagId,
            FeedbackId = model.UserFeedbackId,
        };

        return retVal;
    }

    /// <summary>
    /// Creates a list of UI models from a collection of DB models.
    /// </summary>
    /// <param name="models">The collection of DB models to convert.</param>
    /// <returns>The created list of UI models.</returns>
    public static IList<UserFeedbackTagView> ToUiModel(this IEnumerable<Database.Models.UserFeedbackTag> models)
    {
        return models.Select(ToUiModel).ToList();
    }

    /// <summary>
    /// Creates a DB model from a UI model.
    /// </summary>
    /// <param name="model">The UI model to convert.</param>
    /// <returns>The created DB model.</returns>
    public static Database.Models.UserFeedbackTag ToDbModel(this UserFeedbackTagView model)
    {
        Database.Models.UserFeedbackTag retVal = new()
        {
            UserFeedbackTagId = model.Id,
            Version = model.Version,
        };

        return retVal;
    }
}
