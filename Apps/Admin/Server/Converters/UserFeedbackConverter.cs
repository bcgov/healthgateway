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

using HealthGateway.Admin.Common.Models;

/// <summary>
/// Converts between DB and UI models.
/// </summary>
public static class UserFeedbackConverter
{
    /// <summary>
    /// Creates a UI model from a DB model.
    /// </summary>
    /// <param name="model">The DB model to convert.</param>
    /// <param name="email">The email address associated with the person who created the feedback.</param>
    /// <returns>The created UI model.</returns>
    public static UserFeedbackView ToUiModel(this Database.Models.UserFeedback model, string email)
    {
        UserFeedbackView retVal = new(model.Tags.ToUiModel())
        {
            Id = model.Id,
            UserProfileId = model.UserProfileId,
            Comment = model.Comment,
            CreatedDateTime = model.CreatedDateTime,
            IsReviewed = model.IsReviewed,
            IsSatisfied = model.IsSatisfied,
            Version = model.Version,
            Email = email,
        };

        return retVal;
    }

    /// <summary>
    /// Creates a DB model from a UI model.
    /// </summary>
    /// <param name="model">The UI model to convert.</param>
    /// <returns>The created DB model.</returns>
    public static Database.Models.UserFeedback ToDbModel(this UserFeedbackView model)
    {
        Database.Models.UserFeedback retVal = new()
        {
            Id = model.Id,
            Comment = model.Comment,
            CreatedDateTime = model.CreatedDateTime,
            IsReviewed = model.IsReviewed,
            IsSatisfied = model.IsSatisfied,
            Version = model.Version,
        };

        return retVal;
    }
}
