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
namespace HealthGateway.WebClient.Test.Delegates
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using HealthGateway.WebClient.Delegates;
    using HealthGateway.WebClient.Models;
    using HealthGateway.Common.Models;
    using System.Threading.Tasks;
    using Moq.Protected;
    using System.Threading;
    using System.Net;
    using HealthGateway.Laboratory.Delegates;
    using System.Text.Json;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Constants;
    using HealthGateway.Database.Models;

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
            string json = @"{""smsEnabled"": true, ""smsCellNumber"": ""5551231234"", ""smsScope"": [""COVID19""], ""emailEnabled"": true, ""emailAddress"": ""email@email.blah"", ""emailScope"": [""COVID19""]}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>(){
                ResourcePayload = JsonSerializer.Deserialize<NotificationSettings>(json, options),
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateGetNotificationSettings204()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
            {
                ResourcePayload = new NotificationSettings(),
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateGetNotificationSettings403()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings200()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new NotificationSettings()
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
            string json = JsonSerializer.Serialize(expected.ResourcePayload, options);
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(expected.ResourcePayload, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings201()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new NotificationSettings()
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
            string json = JsonSerializer.Serialize(expected.ResourcePayload, options);
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(expected.ResourcePayload, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings400()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Bad Request, HTTP Error BadRequest",
            };
            NotificationSettings notificationSettings = new NotificationSettings()
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        [Fact]
        public void ValidateSetNotificationSettings403()
        {
            RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden",
            };
            NotificationSettings notificationSettings = new NotificationSettings()
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
            RequestResult<NotificationSettings> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        //[Fact]
        //public async void FunctionalGetNotificationSettings()
        //{
        //    string token = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJVS1ByRFNpSGdXZHM1NXhLc1dVajF6RFZ2alpTdXVib2FNTjZkbUxUcDBNIn0.eyJqdGkiOiIwNDRjYjE4MS1iYjU1LTQzNWUtOThmNC00YTAzYzEwYTVmMmYiLCJleHAiOjE1ODkzMDIxODEsIm5iZiI6MCwiaWF0IjoxNTg5MzAxODgxLCJpc3MiOiJodHRwczovL3Nzby1kZXYucGF0aGZpbmRlci5nb3YuYmMuY2EvYXV0aC9yZWFsbXMvZmYwOXFuM2YiLCJhdWQiOiJoZWFsdGhnYXRld2F5Iiwic3ViIjoiNWQxODg4ZTgtMzVhZS00M2E1LTk3MGItNzI1MjI2YzkyZTA3IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiaGVhbHRoZ2F0ZXdheSIsIm5vbmNlIjoiYjFhMDM0ZjktMjQ2Yi00ZjA0LWFjYjAtNDlmZTVjMGRjNjE1IiwiYXV0aF90aW1lIjoxNTg5MzAxODgwLCJzZXNzaW9uX3N0YXRlIjoiNGY5MThmNjgtNGFkOC00YjQyLTgzYzEtNTYyNDFjMjU3NjYzIiwiYWNyIjoiMSIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL2Rldi5oZWFsdGhnYXRld2F5Lmdvdi5iYy5jYSJdLCJzY29wZSI6Im9wZW5pZCBwYXRpZW50L1BhdGllbnQucmVhZCBhdWRpZW5jZSBvZmZsaW5lX2FjY2VzcyBwYXRpZW50L09ic2VydmF0aW9uLnJlYWQgcGF0aWVudC9NZWRpY2F0aW9uRGlzcGVuc2UucmVhZCIsImhkaWQiOiJRWUZMVkNLN0dKTkwyVDJERTMzRkRBWDdaNUU1UTc0MkUySE9YQjRPQUhaMzJXSTZZWFBBIn0.DN2FS_LnUmUFvnN56_Igcwqkl6Lf9eezXDo96wt1yW1CCfowBRijemBhH0SYWJAzFROPtqRq1UsHPAumfAwB7pPedqnOzhGqY2QXEmk7rGdA0UNvkoGsvTGC7n9R3Z4wW1zUeHLHG9l-QvNIkiTsU1KFBViCKCRCas4B2OY0Io0lz_uj6gMtrC4WedaMR5HJq2r5oai6y1DssKsp6YmYG29wOn8lp3szNOOn2zk-Fkt8AdiaAwziGCUCaip8g6tmaqBCI71qOFpu5xluSU4PA9CMNv8M6n-Ul55NcoV1yXPqi3vPNNg4MHhvyMOSKcvG7MR5sAnTQA2l9eANoxaTiA";
        //    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        //    Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
        //    mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient());
        //    INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);

        //    RequestResult<NotificationSettings> actualResult = await nsDelegate.GetNotificationSettings(token).ConfigureAwait(true);
        //    Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success);
        //}

        //[Fact]
        //public async void FunctionalSetNotificationSettings()
        //{
        //    RequestResult<NotificationSettings> expected = new RequestResult<NotificationSettings>()
        //    {
        //        ResultStatus = Common.Constants.ResultType.Success,
        //        ResourcePayload = new NotificationSettings()
        //        {
        //            SMSEnabled = true,
        //            SMSNumber = "5551231234",
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
        //    RequestResult<NotificationSettings> actualResult = await nsDelegate.SetNotificationSettings(expected.ResourcePayload, token).ConfigureAwait(true);
        //    Assert.True(actualResult.IsDeepEqual(expected));
        //}

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
