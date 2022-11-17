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
namespace HealthGateway.DBMaintainer.FileDownload
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The File Download service for retrieving (binary) files (any mime types).
    /// </summary>
    public class FileDownloadService : IFileDownloadService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<FileDownloadService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadService"/> class.
        /// FileDownloadService constructor.
        /// </summary>
        /// <param name="logger">ILogger instance.</param>
        /// <param name="httpClientFactory">The HTTP Client factory.</param>
        public FileDownloadService(ILogger<FileDownloadService> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Generalize exception block")]
        [SuppressMessage("ReSharper", "UseAwaitUsing", Justification = "awaiting using causes ConfigureAwait warning")]
        public async Task<FileDownload> GetFileFromUrl(Uri fileUrl, string targetFolder, bool isRelativePath)
        {
            FileDownload fd = new();

            if (isRelativePath)
            {
                targetFolder = Path.Combine(Directory.GetCurrentDirectory(), targetFolder);
            }

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            fd.Name = Path.GetRandomFileName() + Path.GetExtension(fileUrl.ToString());
            fd.LocalFilePath = targetFolder;
            string filePath = Path.Combine(fd.LocalFilePath, fd.Name);
            try
            {
                using (HttpClient client = this.httpClientFactory.CreateClient())
                {
                    using Stream inStream = await client.GetStreamAsync(fileUrl).ConfigureAwait(true);
                    using Stream outStream = File.Open(filePath, FileMode.OpenOrCreate);
                    await inStream.CopyToAsync(outStream).ConfigureAwait(true);
                }

                using Stream hashStream = File.OpenRead(filePath);
                using SHA256 mySha256 = SHA256.Create();
                byte[] hashValue = await mySha256.ComputeHashAsync(hashStream).ConfigureAwait(true);
                fd.Hash = Convert.ToBase64String(hashValue);
            }
            catch (Exception exception)
            {
                this.logger.LogCritical("{Exception}", exception.ToString());
                File.Delete(filePath);
                fd.Name = string.Empty;
                fd.LocalFilePath = string.Empty;
                fd.Hash = string.Empty;
            }

            return fd;
        }
    }
}
