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
namespace HealthGateway.Common.ErrorHandling;

using System;
using HealthGateway.Common.Data.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Information associated with a <see cref="ErrorHandling.ProblemType"/>.
/// </summary>
public class Problem
{
    private static readonly Lazy<Problem> InvalidInput =
        new(() => new Problem(ProblemType.InvalidInput, "Invalid input provided", StatusCodes.Status400BadRequest, LogLevel.Information));

    private static readonly Lazy<Problem> Forbidden =
        new(() => new Problem(ProblemType.Forbidden, "Not authorized to perform this operation", StatusCodes.Status403Forbidden, LogLevel.Information));

    private static readonly Lazy<Problem> RecordNotFound =
        new(() => new Problem(ProblemType.RecordNotFound, "Record was not found", StatusCodes.Status404NotFound, LogLevel.Information));

    private static readonly Lazy<Problem> RecordAlreadyExists =
        new(() => new Problem(ProblemType.RecordAlreadyExists, "Record already exists", StatusCodes.Status409Conflict, LogLevel.Information));

    private static readonly Lazy<Problem> ServerError =
        new(() => new Problem(ProblemType.ServerError, "An error occurred", StatusCodes.Status500InternalServerError, LogLevel.Error));

    private static readonly Lazy<Problem> DatabaseError =
        new(() => new Problem(ProblemType.DatabaseError, "A database error occurred", StatusCodes.Status500InternalServerError, LogLevel.Error));

    private static readonly Lazy<Problem> InvalidData =
        new(() => new Problem(ProblemType.InvalidData, "Invalid data was returned", StatusCodes.Status500InternalServerError, LogLevel.Information));

    private static readonly Lazy<Problem> UpstreamError =
        new(() => new Problem(ProblemType.UpstreamError, "An error occurred with an upstream service", StatusCodes.Status502BadGateway, LogLevel.Warning));

    private static readonly Lazy<Problem> MaxRetriesReached =
        new(() => new Problem(ProblemType.MaxRetriesReached, "Maximum retry attempts reached", StatusCodes.Status502BadGateway, LogLevel.Warning));

    private static readonly Lazy<Problem> RefreshInProgress =
        new(() => new Problem(ProblemType.RefreshInProgress, "Data is in the process of being refreshed", StatusCodes.Status502BadGateway, LogLevel.Information));

    private Problem(ProblemType problemType, string title, int statusCode, LogLevel logLevel)
    {
        this.ProblemType = problemType;
        this.Title = title;
        this.StatusCode = statusCode;
        this.LogLevel = logLevel;
    }

    /// <summary>
    /// Gets the <see cref="ProblemType"/> that corresponds to the problem.
    /// </summary>
    public ProblemType ProblemType { get; }

    /// <summary>
    /// Gets a succinct human-readable summary of the problem.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the HTTP status code associated with the problem.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the level at which to log exceptions associated with this problem.
    /// </summary>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// Gets the unique tag URI for the problem.
    /// </summary>
    public Uri TagUri => new($"tag:healthgateway.gov.bc.ca,2024:{EnumUtility.ToEnumString(this.ProblemType, true)}");

    /// <summary>
    /// Returns the <see cref="Problem"/> instance that corresponds to a given <see cref="ErrorHandling.ProblemType"/>.
    /// </summary>
    /// <param name="problemType">The problem type.</param>
    /// <returns>The <see cref="Problem"/> instance that corresponds to the given <see cref="ErrorHandling.ProblemType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Unknown problem type.</exception>
    public static Problem Get(ProblemType problemType)
    {
        return problemType switch
        {
            ProblemType.InvalidInput => InvalidInput.Value,
            ProblemType.Forbidden => Forbidden.Value,
            ProblemType.RecordNotFound => RecordNotFound.Value,
            ProblemType.RecordAlreadyExists => RecordAlreadyExists.Value,
            ProblemType.ServerError => ServerError.Value,
            ProblemType.DatabaseError => DatabaseError.Value,
            ProblemType.InvalidData => InvalidData.Value,
            ProblemType.UpstreamError => UpstreamError.Value,
            ProblemType.MaxRetriesReached => MaxRetriesReached.Value,
            ProblemType.RefreshInProgress => RefreshInProgress.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(problemType), problemType, "Unknown problem type"),
        };
    }
}
