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
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class FileDownloadService_Test
    {
        [Fact]
        public void ShouldGetFile()
        {
            string response = "A simple file";
            string md5Hash = "ur2T7CGMUXLkYT+IgN4Xy8r33EktY9JrTOm0FZwhh1A="; // This hash has to change if you change the response above
            string targetFolder = "tmp";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FileDownloadService> logger = loggerFactory.CreateLogger<FileDownloadService>();
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
                   Content = new StringContent(response),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            FileDownloadService service = new FileDownloadService(logger, mockHttpClientService.Object);
            FileDownload fd = Task.Run(async () => await service.GetFileFromUrl(new System.Uri("https://localhost/fake.txt"), targetFolder, true).ConfigureAwait(true)).Result;
            string filename = Path.Combine(fd.LocalFilePath, fd.Name);
            
            // Clean up physical artifacts
            File.Delete(filename);
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), targetFolder));
            
            Assert.True(fd.Hash == md5Hash);
        }

        [Fact]
        public void ShouldCauseException()
        {
            string targetFolder = "tmp";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FileDownloadService> logger = loggerFactory.CreateLogger<FileDownloadService>();
            FileDownloadService service = new FileDownloadService(logger, null);
            FileDownload fd = Task.Run(async () => await service.GetFileFromUrl(new System.Uri("https://localhost/fake.txt"), "tmp", true).ConfigureAwait(true)).Result;
            Assert.ThrowsAsync<AggregateException>(() => Task.Run(async () => await service.GetFileFromUrl(new System.Uri("https://localhost/fake.txt"), targetFolder, true).ConfigureAwait(true)));
            Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), targetFolder));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"Section:Config", ""},
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
