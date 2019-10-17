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
    /// <summary>
    /// The DownloadedFile containing local file path, and md5 resulting from a service request.
    /// </summary>
    public class DownloadedFile
    {
        /// <summary>
        /// Gets or sets the local file path to store the downloaded file.
        /// </summary>
        public string  LocalFilePath { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 Hash of the file saved to the local file path.
        /// </summary>
        public string FileSHA256 { get; set; }
    }
}