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
namespace HealthGateway.CommonTests.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using Xunit;

    /// <summary>
    /// HttpContextHelper's Unit Tests.
    /// </summary>
    public class HttpContextHelperTests
    {
        private const string RouteHdid = "ROUTE-HDID";
        private const string QueryParamHdid = "QUERY-PARAM-HDID";

        /// <summary>
        /// Should GetResourceHdid.
        /// </summary>
        /// <param name="lookupMethod">The mechanism with which to retrieve the subject identifier.</param>
        [InlineData(FhirSubjectLookupMethod.Route)]
        [InlineData(FhirSubjectLookupMethod.Parameter)]
        [InlineData((FhirSubjectLookupMethod)999)]
        [Theory]
        public void ShouldGetResourceHdid(FhirSubjectLookupMethod lookupMethod)
        {
            // Arrange
            string? expected = lookupMethod switch
            {
                FhirSubjectLookupMethod.Parameter => QueryParamHdid,
                FhirSubjectLookupMethod.Route => RouteHdid,
                _ => null,
            };

            Mock<HttpContext> httpContextMock = SetupHttpContextForGetResourceHdid(lookupMethod);

            // Act
            string? actual = HttpContextHelper.GetResourceHdid(httpContextMock.Object, lookupMethod);

            // Assert
            Assert.Equal(expected, actual);
        }

        [SuppressMessage("Style", "IDE0010:Populate switch", Justification = "Team decision")]
        private static Mock<HttpContext> SetupHttpContextForGetResourceHdid(FhirSubjectLookupMethod lookupMethod)
        {
            Mock<HttpRequest> httpRequestMock = new();

            switch (lookupMethod)
            {
                case FhirSubjectLookupMethod.Parameter:
                    {
                        QueryCollection queryCollection = new(
                            new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
                            {
                            { "Hdid", QueryParamHdid }, // Use Hdid for hdid in HttpContextHelper to check for case insensitivity
                            });

                        httpRequestMock.Setup(s => s.Query).Returns(queryCollection);
                        break;
                    }

                case FhirSubjectLookupMethod.Route:
                    {
                        RouteValueDictionary routeValues = new()
                    {
                        { "Hdid", RouteHdid }, // Use Hdid for hdid in HttpContextHelper to check for case insensitivity
                    };

                        httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);
                        break;
                    }
            }

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            return httpContextMock;
        }
    }
}
