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
        private readonly ILogger<FileDownloadService> logger;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// FileDownloadService constructor
        /// </summary>
        /// <param name="logger">ILogger instance</param>
        /// <param name="httpClientFactory">IHttpClientFactory instance</param>
        public FileDownloadService(ILogger<FileDownloadService> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public async Task<DownloadedFile> GetFileFromUrl(Uri url, string targetFolder, bool isRelativePath)
        {
            DownloadedFile df = new DownloadedFile();

            if (isRelativePath)
            {
                targetFolder = Path.Combine(Directory.GetCurrentDirectory(), targetFolder);
            }

            df.FileName = Path.GetRandomFileName() + Path.GetExtension(url.ToString());
            df.LocalFilePath = targetFolder;

            string filePath = Path.Combine(df.LocalFilePath, df.FileName);

            try
            {
                using (HttpClient client = this.httpClientFactory.CreateClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(true);

                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        using (Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(true))
                        {
                            using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                            {
                                await stream.CopyToAsync(fileStream).ConfigureAwait(true);
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
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger.LogDebug(exception.ToString());
                File.Delete(filePath);
                df.FileName = string.Empty;
                df.LocalFilePath = string.Empty;
                df.FileSHA256 = string.Empty;
            }

            return df;
        }
    }
}