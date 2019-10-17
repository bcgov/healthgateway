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
namespace HealthGateway.Common.FileDownload
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The File Download service for retrieving (binary) files (any mime types).
    /// </summary>
    public class FileDownloadService : IFileDownloadService
    {
        private ILogger<FileDownloadService> logger;
        private string targetFolder;
        private readonly HttpClient httpClient;

        /// <summary>
        /// FileDownloadService constructor
        /// </summary>
        /// <param name="logger">ILogger instance</param>
        /// <param name="httpClient">HttpClient instance</param>
        /// <param name="targetFolder">full folder path to target folder</param>
        public FileDownloadService(ILogger<FileDownloadService> logger, HttpClient httpClient, string targetFolder)
        {
            this.logger = logger;
            this.targetFolder = targetFolder;
            this.httpClient = httpClient;
        }
        /// <inheritdoc/>
        public async Task<DownloadedFile> GetFileFromUrl(string url)
        {
            DownloadedFile df = new DownloadedFile();

            string filePath = Path.Combine(targetFolder, Path.GetRandomFileName());

            df.LocalFilePath = filePath;

            try
            {
                using (Stream stream = await this.httpClient.GetStreamAsync(url).ConfigureAwait(false))
                {
                    using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                    {
                        await stream.CopyToAsync(fileStream);
                        using (SHA256 mySHA256 = SHA256.Create())
                        {
                            fileStream.Position = 0;
                            byte[] hashValue = mySHA256.ComputeHash(fileStream);
                            df.FileSHA256 = Encoding.UTF8.GetString(hashValue, 0, hashValue.Length);
                        }
                        fileStream.Close();
                    }
                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                this.logger.LogDebug(exception.ToString());
                File.Delete(filePath);
                df.LocalFilePath = string.Empty;
                df.FileSHA256 = string.Empty;
            }

            return df;

        }
    }
}