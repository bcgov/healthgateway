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
namespace HealthGateway.CommonTests.FileDownload
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// FileDownloadService's Unit Tests.
    /// </summary>
    public class FileDownloadServiceTests
    {
        /// <summary>
        /// GetFile - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetFile()
        {
            string response = "A simple file";
            string md5Hash = "ur2T7CGMUXLkYT+IgN4Xy8r33EktY9JrTOm0FZwhh1A="; // This hash has to change if you change the response above
            string targetFolder = "tmp";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FileDownloadService> logger = loggerFactory.CreateLogger<FileDownloadService>();

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response),
            };
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            FileDownloadService service = new(logger, mockHttpClientService.Object);
            FileDownload fd = Task.Run(async () => await service.GetFileFromUrl(new Uri("https://localhost/fake.txt"), targetFolder, true).ConfigureAwait(true)).Result;
            string filename = Path.Combine(fd.LocalFilePath, fd.Name);

            // Clean up physical artifacts
            File.Delete(filename);
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), targetFolder));

            Assert.True(fd.Hash == md5Hash);
        }

        /// <summary>
        /// GetFile - Unknown Exception.
        /// </summary>
        [Fact]
        public void ShouldCauseException()
        {
            string targetFolder = "tmp";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FileDownloadService> logger = loggerFactory.CreateLogger<FileDownloadService>();
            FileDownloadService service = new(logger, new Mock<IHttpClientService>().Object);
            Task.Run(async () => await service.GetFileFromUrl(new Uri("https://localhost/fake.txt"), "tmp", true).ConfigureAwait(true));
            Assert.ThrowsAsync<AggregateException>(() => Task.Run(async () => await service.GetFileFromUrl(new Uri("https://localhost/fake.txt"), targetFolder, true).ConfigureAwait(true)));
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), targetFolder));
        }
    }
}
