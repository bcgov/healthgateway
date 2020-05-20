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
namespace HealthGateway.CommonTests.Delegates
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using System.Threading.Tasks;
    using Moq.Protected;
    using System.Threading;
    using System.Net;
    using System.Text.Json;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;

    public class NotificationSettingsDelegate_Test
    {

        private readonly IConfiguration configuration;

        public NotificationSettingsDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void ValidateGetNotificationSettings200()
        {
            string json = @"{""smsEnabled"": true, ""smsCellNumber"": ""5551231234"", ""smsVerified"": true, ""smsScope"": [""COVID19""], ""emailEnabled"": true, ""emailAddress"": ""email@email.blah"", ""emailScope"": [""COVID19""]}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>(){
                ResourcePayload = JsonSerializer.Deserialize<NotificationSettingsResponse>(json, options),
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(json),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateGetNotificationSettings204()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResourcePayload = new NotificationSettingsResponse(),
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 0,
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.NoContent,
                   Content = new StringContent(string.Empty),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateGetNotificationSettings403()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden",
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.Forbidden,
                   Content = new StringContent(string.Empty),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings200()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new NotificationSettingsResponse()
                {
                    SMSEnabled = true,
                    SMSNumber = "5551231234",
                    SMSScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                    EmailEnabled = true,
                    EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                    EmailScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                },
                TotalResultCount = 1,
            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            NotificationSettingsRequest request = new NotificationSettingsRequest(expected.ResourcePayload);
            request.SMSVerificationCode = "1234";
            string json = JsonSerializer.Serialize(request, options);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(json),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings201()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new NotificationSettingsResponse()
                {
                    SMSEnabled = true,
                    SMSNumber = "5551231234",
                    SMSScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                    EmailEnabled = true,
                    EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                    EmailScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                },
                TotalResultCount = 1,
            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            NotificationSettingsRequest request = new NotificationSettingsRequest(expected.ResourcePayload);
            request.SMSVerificationCode = "1234";
            string json = JsonSerializer.Serialize(request, options);
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.Created,
                   Content = new StringContent(json),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings400()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Bad Request, HTTP Error BadRequest",
            };
            NotificationSettingsRequest notificationSettings = new NotificationSettingsRequest()
            {
                SMSEnabled = true,
                SMSNumber = "5551231234",
                SMSScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                EmailEnabled = true,
                EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                EmailScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.BadRequest,
                   Content = new StringContent(string.Empty),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings403()
        {
            RequestResult<NotificationSettingsRequest> expected = new RequestResult<NotificationSettingsRequest>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden",
            };
            NotificationSettingsRequest notificationSettings = new NotificationSettingsRequest()
            {
                SMSEnabled = true,
                SMSNumber = "5551231234",
                SMSScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                EmailEnabled = true,
                EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                EmailScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
            };
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.Forbidden,
                   Content = new StringContent(string.Empty),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        //[Fact]
        //public async void FunctionalGetNotificationSettings()
        //{
        //    RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
        //    {
        //        ResultStatus = Common.Constants.ResultType.Success,
        //        ResourcePayload = new NotificationSettingsResponse()
        //        {
        //            SMSEnabled = true,
        //            SMSNumber = "6042026997",
        //            SMSVerified = true,
        //            SMSScope = new List<NotificationTarget>
        //            {
        //                NotificationTarget.Covid19,
        //            },
        //            EmailEnabled = true,
        //            EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
        //            EmailScope = new List<NotificationTarget>
        //            {
        //                NotificationTarget.Covid19,
        //            },
        //        },
        //        TotalResultCount = 1,
        //    };
        //    string token = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJVS1ByRFNpSGdXZHM1NXhLc1dVajF6RFZ2alpTdXVib2FNTjZkbUxUcDBNIn0.eyJqdGkiOiIwNDRjYjE4MS1iYjU1LTQzNWUtOThmNC00YTAzYzEwYTVmMmYiLCJleHAiOjE1ODkzMDIxODEsIm5iZiI6MCwiaWF0IjoxNTg5MzAxODgxLCJpc3MiOiJodHRwczovL3Nzby1kZXYucGF0aGZpbmRlci5nb3YuYmMuY2EvYXV0aC9yZWFsbXMvZmYwOXFuM2YiLCJhdWQiOiJoZWFsdGhnYXRld2F5Iiwic3ViIjoiNWQxODg4ZTgtMzVhZS00M2E1LTk3MGItNzI1MjI2YzkyZTA3IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiaGVhbHRoZ2F0ZXdheSIsIm5vbmNlIjoiYjFhMDM0ZjktMjQ2Yi00ZjA0LWFjYjAtNDlmZTVjMGRjNjE1IiwiYXV0aF90aW1lIjoxNTg5MzAxODgwLCJzZXNzaW9uX3N0YXRlIjoiNGY5MThmNjgtNGFkOC00YjQyLTgzYzEtNTYyNDFjMjU3NjYzIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL2Rldi5oZWFsdGhnYXRld2F5Lmdvdi5iYy5jYSJdLCJzY29wZSI6Im9wZW5pZCBwYXRpZW50L1BhdGllbnQucmVhZCBhdWRpZW5jZSBvZmZsaW5lX2FjY2VzcyBwYXRpZW50L09ic2VydmF0aW9uLnJlYWQgcGF0aWVudC9NZWRpY2F0aW9uRGlzcGVuc2UucmVhZCIsImhkaWQiOiJRWUZMVkNLN0dKTkwyVDJERTMzRkRBWDdaNUU1UTc0MkUySE9YQjRPQUhaMzJXSTZZWFBBIn0.DN2FS_LnUmUFvnN56_Igcwqkl6Lf9eezXDo96wt1yW1CCfowBRijemBhH0SYWJAzFROPtqRq1UsHPAumfAwB7pPedqnOzhGqY2QXEmk7rGdA0UNvkoGsvTGC7n9R3Z4wW1zUeHLHG9l-QvNIkiTsU1KFBViCKCRCas4B2OY0Io0lz_uj6gMtrC4WedaMR5HJq2r5oai6y1DssKsp6YmYG29wOn8lp3szNOOn2zk-Fkt8AdiaAwziGCUCaip8g6tmaqBCI71qOFpu5xluSU4PA9CMNv8M6n-Ul55NcoV1yXPqi3vPNNg4MHhvyMOSKcvG7MR5sAnTQA2l9eANoxaTiA";
        //    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        //    Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
        //    mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient());
        //    INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);

        //    RequestResult<NotificationSettingsResponse> actualResult = await nsDelegate.GetNotificationSettings(token).ConfigureAwait(true);
        //    Assert.True(actualResult.IsDeepEqual(expected));
        //}

        //[Fact]
        //public async void FunctionalSetNotificationSettings()
        //{
        //    RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
        //    {
        //        ResultStatus = Common.Constants.ResultType.Success,
        //        ResourcePayload = new NotificationSettingsResponse()
        //        {
        //            SMSEnabled = true,
        //            SMSNumber = "6042026997",
        //            SMSVerified = true,
        //            SMSScope = new List<NotificationTarget>
        //            {
        //                NotificationTarget.Covid19,
        //            },
        //            EmailEnabled = true,
        //            EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
        //            EmailScope = new List<NotificationTarget>
        //            {
        //                NotificationTarget.Covid19,
        //            },
        //        },
        //        TotalResultCount = 1,
        //    };
        //    NotificationSettingsRequest request = new NotificationSettingsRequest(expected.ResourcePayload);
        //    request.SMSVerificationCode = "1234";
        //    request.SMSNumber = "6042026997";
        //    string token = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJVS1ByRFNpSGdXZHM1NXhLc1dVajF6RFZ2alpTdXVib2FNTjZkbUxUcDBNIn0.eyJqdGkiOiIwNDRjYjE4MS1iYjU1LTQzNWUtOThmNC00YTAzYzEwYTVmMmYiLCJleHAiOjE1ODkzMDIxODEsIm5iZiI6MCwiaWF0IjoxNTg5MzAxODgxLCJpc3MiOiJodHRwczovL3Nzby1kZXYucGF0aGZpbmRlci5nb3YuYmMuY2EvYXV0aC9yZWFsbXMvZmYwOXFuM2YiLCJhdWQiOiJoZWFsdGhnYXRld2F5Iiwic3ViIjoiNWQxODg4ZTgtMzVhZS00M2E1LTk3MGItNzI1MjI2YzkyZTA3IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiaGVhbHRoZ2F0ZXdheSIsIm5vbmNlIjoiYjFhMDM0ZjktMjQ2Yi00ZjA0LWFjYjAtNDlmZTVjMGRjNjE1IiwiYXV0aF90aW1lIjoxNTg5MzAxODgwLCJzZXNzaW9uX3N0YXRlIjoiNGY5MThmNjgtNGFkOC00YjQyLTgzYzEtNTYyNDFjMjU3NjYzIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL2Rldi5oZWFsdGhnYXRld2F5Lmdvdi5iYy5jYSJdLCJzY29wZSI6Im9wZW5pZCBwYXRpZW50L1BhdGllbnQucmVhZCBhdWRpZW5jZSBvZmZsaW5lX2FjY2VzcyBwYXRpZW50L09ic2VydmF0aW9uLnJlYWQgcGF0aWVudC9NZWRpY2F0aW9uRGlzcGVuc2UucmVhZCIsImhkaWQiOiJRWUZMVkNLN0dKTkwyVDJERTMzRkRBWDdaNUU1UTc0MkUySE9YQjRPQUhaMzJXSTZZWFBBIn0.DN2FS_LnUmUFvnN56_Igcwqkl6Lf9eezXDo96wt1yW1CCfowBRijemBhH0SYWJAzFROPtqRq1UsHPAumfAwB7pPedqnOzhGqY2QXEmk7rGdA0UNvkoGsvTGC7n9R3Z4wW1zUeHLHG9l-QvNIkiTsU1KFBViCKCRCas4B2OY0Io0lz_uj6gMtrC4WedaMR5HJq2r5oai6y1DssKsp6YmYG29wOn8lp3szNOOn2zk-Fkt8AdiaAwziGCUCaip8g6tmaqBCI71qOFpu5xluSU4PA9CMNv8M6n-Ul55NcoV1yXPqi3vPNNg4MHhvyMOSKcvG7MR5sAnTQA2l9eANoxaTiA";
        //    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        //    Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
        //    mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient());
        //    INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
        //    RequestResult<NotificationSettingsResponse> actualResult = await nsDelegate.SetNotificationSettings(request, token).ConfigureAwait(true);
        //    Assert.True(actualResult.IsDeepEqual(expected));
        //}

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"NotificationSettings:Endpoint", "https://phsahealthgatewayapi.azurewebsites.net/api/v1/Settings/Notification"},
            };

            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
