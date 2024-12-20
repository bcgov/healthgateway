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
namespace HealthGateway.CommonTests.ErrorHandling
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.Common.ErrorHandling.ExceptionHandlers;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for all exception handler classes.
    /// </summary>
    public class ExceptionHandlerTests
    {
        /// <summary>
        /// Gets parameters for DefaultExceptionHandler unit test(s).
        /// </summary>
        public static TheoryData<Exception> DefaultExceptionHandlerTheoryData =>
        [
            new UnauthorizedAccessException(),
            new CommunicationException(),
            new AlreadyExistsException(),
            new DatabaseException(),
            new InvalidDataException(),
            new NotFoundException(),
            new UpstreamServiceException(),
            new("e", new("e", new("e", new("e", new("e", new("e", new("e", new("e", new("e", new("e")))))))))),
        ];

        /// <summary>
        /// DefaultExceptionHandler TryHandleAsync - Happy Path.
        /// </summary>
        /// <param name="exception">The exception to be handled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [MemberData(nameof(DefaultExceptionHandlerTheoryData))]
        public async Task ValidateDefaultExceptionHandlerTryHandleAsync(Exception exception)
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();

            DefaultExceptionHandlerSetup setup = GetDefaultExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// ApiExceptionHandler TryHandleAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateApiExceptionHandlerTryHandleAsync()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            using StringContent httpContent = new("content");
            Exception exception = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.InternalServerError, HttpMethod.Get, httpContent);

            ApiExceptionHandlerSetup setup = GetApiExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// ApiExceptionHandler TryHandleAsync - Unsupported Exception Type.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateApiExceptionHandlerTryHandleAsyncUnsupportedException()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            Exception exception = new InvalidOperationException();

            ApiExceptionHandlerSetup setup = GetApiExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// DbUpdateExceptionHandler TryHandleAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateDbUpdateExceptionHandlerTryHandleAsync()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            using StringContent httpContent = new("content");
            Exception exception = new DbUpdateException("DB Error");

            DbUpdateExceptionHandlerSetup setup = GetDbUpdateExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// DbUpdateExceptionHandler TryHandleAsync - Unsupported Exception Type.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateDbUpdateExceptionHandlerTryHandleAsyncUnsupportedException()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            Exception exception = new InvalidOperationException();

            DbUpdateExceptionHandlerSetup setup = GetDbUpdateExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.False(actual);
        }

        /// <summary>
        /// ValidationExceptionHandler TryHandleAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateValidationExceptionHandlerTryHandleAsync()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            Exception exception = new FluentValidation.ValidationException([new ValidationFailure("hdid", "required"), new ValidationFailure("hdid", "malformed")]);

            ValidationExceptionHandlerSetup setup = GetValidationExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.True(actual);
        }

        /// <summary>
        /// ValidationExceptionHandler TryHandleAsync - Unsupported Exception Type.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateValidationExceptionHandlerTryHandleAsyncUnsupportedException()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            Exception exception = new InvalidOperationException();

            ValidationExceptionHandlerSetup setup = GetValidationExceptionHandlerSetup(true);

            // Act
            bool actual = await setup.ExceptionHandler.TryHandleAsync(httpContext, exception, default);

            // Assert
            Assert.False(actual);
        }

        private static ApiExceptionHandlerSetup GetApiExceptionHandlerSetup(bool includeExceptionDetailsInResponse)
        {
            ApiExceptionHandlerMocks mocks = new(new());
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("IncludeExceptionDetailsInResponse", includeExceptionDetailsInResponse.ToString()),
                ])
                .Build();

            return new(GetApiExceptionHandler(mocks, configuration));
        }

        private static DbUpdateExceptionHandlerSetup GetDbUpdateExceptionHandlerSetup(bool includeExceptionDetailsInResponse)
        {
            DbUpdateExceptionHandlerMocks mocks = new(new());
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("IncludeExceptionDetailsInResponse", includeExceptionDetailsInResponse.ToString()),
                ])
                .Build();

            return new(GetDbUpdateExceptionHandler(mocks, configuration));
        }

        private static DefaultExceptionHandlerSetup GetDefaultExceptionHandlerSetup(bool includeExceptionDetailsInResponse)
        {
            DefaultExceptionHandlerMocks mocks = new(new());
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("IncludeExceptionDetailsInResponse", includeExceptionDetailsInResponse.ToString()),
                ])
                .Build();

            return new(GetDefaultExceptionHandler(mocks, configuration));
        }

        private static ValidationExceptionHandlerSetup GetValidationExceptionHandlerSetup(bool includeExceptionDetailsInResponse)
        {
            ValidationExceptionHandlerMocks mocks = new(new());
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("IncludeExceptionDetailsInResponse", includeExceptionDetailsInResponse.ToString()),
                ])
                .Build();

            return new(GetValidationExceptionHandler(mocks, configuration));
        }

        private static ProblemDetails GenerateProblemDetails(HttpContext httpContext, int? statusCode, string? title, string? type, string? detail, string? instance)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            };
        }

        private static ValidationProblemDetails GenerateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode,
            string? title,
            string? type,
            string? detail,
            string? instance)
        {
            return new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier,
                },
            };
        }

        private static ProblemDetailsFactory GetProblemDetailsFactory()
        {
            Mock<ProblemDetailsFactory> problemDetailsFactory = new();
            problemDetailsFactory
                .Setup(s => s.CreateProblemDetails(It.IsAny<HttpContext>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns(GenerateProblemDetails);
            problemDetailsFactory
                .Setup(
                    s => s.CreateValidationProblemDetails(
                        It.IsAny<HttpContext>(),
                        It.IsAny<ModelStateDictionary>(),
                        It.IsAny<int?>(),
                        It.IsAny<string?>(),
                        It.IsAny<string?>(),
                        It.IsAny<string?>(),
                        It.IsAny<string?>()))
                .Returns(GenerateValidationProblemDetails);

            return problemDetailsFactory.Object;
        }

        private static ApiExceptionHandler GetApiExceptionHandler(ApiExceptionHandlerMocks mocks, IConfiguration configuration)
        {
            return new(configuration, mocks.Logger.Object, GetProblemDetailsFactory());
        }

        private static DbUpdateExceptionHandler GetDbUpdateExceptionHandler(DbUpdateExceptionHandlerMocks mocks, IConfiguration configuration)
        {
            return new(configuration, mocks.Logger.Object, GetProblemDetailsFactory());
        }

        private static DefaultExceptionHandler GetDefaultExceptionHandler(DefaultExceptionHandlerMocks mocks, IConfiguration configuration)
        {
            return new(configuration, mocks.Logger.Object, GetProblemDetailsFactory());
        }

        private static ValidationExceptionHandler GetValidationExceptionHandler(ValidationExceptionHandlerMocks mocks, IConfiguration configuration)
        {
            return new(configuration, mocks.Logger.Object, GetProblemDetailsFactory());
        }

        private sealed record ApiExceptionHandlerSetup(ApiExceptionHandler ExceptionHandler);

        private sealed record DbUpdateExceptionHandlerSetup(DbUpdateExceptionHandler ExceptionHandler);

        private sealed record DefaultExceptionHandlerSetup(DefaultExceptionHandler ExceptionHandler);

        private sealed record ValidationExceptionHandlerSetup(ValidationExceptionHandler ExceptionHandler);

        private sealed record ApiExceptionHandlerMocks(Mock<ILogger<ApiExceptionHandler>> Logger);

        private sealed record DbUpdateExceptionHandlerMocks(Mock<ILogger<DbUpdateExceptionHandler>> Logger);

        private sealed record DefaultExceptionHandlerMocks(Mock<ILogger<DefaultExceptionHandler>> Logger);

        private sealed record ValidationExceptionHandlerMocks(Mock<ILogger<ValidationExceptionHandler>> Logger);
    }
}
