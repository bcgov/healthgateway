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
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Interface that defines a file downloader.
    /// </summary>
    public interface IFileDownloadService
    {
        /// <summary>
        /// Service to download a file specified by the supplied URL.
        /// </summary>
        /// <param name="fileUrl">The url of the file to be downloaded.</param>
        /// <param name="targetFolder">Target folder once the download is suscessfull.</param>
        /// <param name="isRelativePath">True if the target folder is a lrelative path.</param>
        /// <returns>The DownloadedFile.</returns>
        Task<FileDownload> GetFileFromUrl(Uri fileUrl, string targetFolder, bool isRelativePath);
    }
}
