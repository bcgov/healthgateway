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
namespace HealthGateway.Admin.Server.Delegates
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Renci.SshNet;

    /// <summary>
    /// A delegate to mail documents over SFTP.
    /// </summary>
    public class SFTPMailDelegate : IMailDelegate
    {
        private const string DocumentStorageConfigurationSectionKey = "DocumentStorage";

        private readonly ILogger logger;
        private readonly DocumentStorageConfiguration documentStorageConfiguration;
        private readonly ConnectionInfo? connectionInfo;
        private readonly bool connectionInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="SFTPMailDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        public SFTPMailDelegate(ILogger<SFTPMailDelegate> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.documentStorageConfiguration = new DocumentStorageConfiguration();
            configuration.Bind(DocumentStorageConfigurationSectionKey, this.documentStorageConfiguration);
            try
            {
                PrivateKeyFile privateKey;
                if (!string.IsNullOrEmpty(this.documentStorageConfiguration.PrivateKeyPassphrase))
                {
                    privateKey = new PrivateKeyFile(
                        this.documentStorageConfiguration.PrivateKeyPath,
                        this.documentStorageConfiguration.PrivateKeyPassphrase);
                }
                else
                {
                    privateKey = new PrivateKeyFile(this.documentStorageConfiguration.PrivateKeyPath);
                }

                this.connectionInfo = new(
                this.documentStorageConfiguration.SftpHostname,
                this.documentStorageConfiguration.SftpPort,
                this.documentStorageConfiguration.SftpUsername,
                new PrivateKeyAuthenticationMethod(
                    this.documentStorageConfiguration.SftpUsername,
                    privateKey));
                this.connectionInitialized = true;
            }
            catch (Exception e)
            {
                this.connectionInfo = null;
                this.logger.LogError($"Unexpected exception initializing connection info in SFTPMailDelegate {e}");
            }
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(SFTPMailDelegate));

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        public PrimitiveRequestResult<bool> SendDocument(ReportModel document)
        {
            using Activity? activity = Source.StartActivity("SendDocument");
            this.logger.LogDebug($"Sending document to mail {document.FileName}...");

            PrimitiveRequestResult<bool> retVal = new();

            if (!this.connectionInitialized)
            {
                retVal.ResourcePayload = false;
                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Connection is not initialized", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                this.logger.LogError($"Document could not be mailed because connection is not initialized");
            }
            else
            {
                byte[] documentBytes = Convert.FromBase64String(document.Data);

                try
                {
                    using MemoryStream documentStream = new(documentBytes);
                    using SftpClient sftpClient = new(this.connectionInfo);

                    sftpClient.Connect();
                    sftpClient.ChangeDirectory(this.documentStorageConfiguration.SftpFolderPath);
                    sftpClient.UploadFile(documentStream, document.FileName);
                    sftpClient.Disconnect();

                    retVal.ResourcePayload = true;
                    retVal.ResultStatus = ResultType.Success;
                }
                catch (Exception e)
                {
                    retVal.ResourcePayload = false;
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception mailing document: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.SFTP) };
                    this.logger.LogError($"Unexpected exception in QueueDocument {e}");
                }
            }

            this.logger.LogDebug($"Finished sending document to mail {document.FileName}...");

            return retVal;
        }
    }
}
