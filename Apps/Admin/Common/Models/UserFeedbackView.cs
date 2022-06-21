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

namespace HealthGateway.Admin.Common.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Model that provides a user representation of a user feedback.
/// </summary>
public class UserFeedbackView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeedbackView"/> class.
    /// </summary>
    public UserFeedbackView()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeedbackView"/> class.
    /// </summary>
    /// <param name="tags">The list of user feedback tags.</param>
    [JsonConstructor]
    public UserFeedbackView(IList<UserFeedbackTagView> tags)
    {
        this.Tags = tags;
    }

    /// <summary>
    /// Gets or sets the user feedback id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the related user profile id.
    /// </summary>
    public string? UserProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the feedback comments.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is satisfied or not.
    /// </summary>
    public bool IsSatisfied { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the feedback is reviewed or not.
    /// </summary>
    public bool IsReviewed { get; set; }

    /// <summary>
    /// Gets or sets the date when the feedback was created.
    /// </summary>
    public DateTime CreatedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the version of the resource.
    /// </summary>
    public uint Version { get; set; }

    /// <summary>
    /// Gets or sets the email if known for this feedback.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets the feedback admin tags.
    /// </summary>
    public IList<UserFeedbackTagView> Tags { get; } = new List<UserFeedbackTagView>();

    /// <summary>
    /// Returns a shallow copy of the object.
    /// </summary>
    /// <returns>A new object containing the same values as the current object.</returns>
    public UserFeedbackView ShallowCopy()
    {
        return (UserFeedbackView)this.MemberwiseClone();
    }
}
