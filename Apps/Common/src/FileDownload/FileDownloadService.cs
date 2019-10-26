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
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
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
        private readonly ILogger<FileDownloadService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDownloadService"/> class.
        /// FileDownloadService constructor.
        /// </summary>
        /// <param name="logger">ILogger instance.</param>
        ///
        public FileDownloadService(ILogger<FileDownloadService> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public FileDownload GetFileFromUrl(Uri fileUrl, string targetFolder, bool isRelativePath)
        {
            Contract.Requires(fileUrl != null);
            FileDownload fd = new FileDownload();

            if (isRelativePath)
            {
                targetFolder = Path.Combine(Directory.GetCurrentDirectory(), targetFolder);
            }

            fd.Name = Path.GetRandomFileName() + Path.GetExtension(fileUrl.ToString());
            fd.LocalFilePath = targetFolder;
            string filePath = Path.Combine(fd.LocalFilePath, fd.Name);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(fileUrl, filePath);
                    using (Stream s = File.OpenRead(filePath))
                    {
                        using (SHA256 mySHA256 = SHA256.Create())
                        {
                            byte[] hashValue = mySHA256.ComputeHash(s);
                            fd.Hash = System.Convert.ToBase64String(hashValue);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger.LogCritical(exception.ToString());
                File.Delete(filePath);
                fd.Name = string.Empty;
                fd.LocalFilePath = string.Empty;
                fd.Hash = string.Empty;
            }

            return fd;
        }
    }
}
