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
namespace HealthGateway.DBMaintainer.Apps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.FileDownload;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The base class for HealthGateway Drug file loading utility programs.
    /// </summary>
    /// <typeparam name="T">The parser to use to process files.</typeparam>
    public abstract class BaseDrugApp<T> : IDrugApp
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDrugApp{T}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="parser">The file parser.</param>
        /// <param name="downloadService">The download utility.</param>
        /// <param name="configuration">The IConfiguration to use.</param>
        /// <param name="drugDbContext">The database context to interact with.</param>
        protected BaseDrugApp(ILogger logger, T parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDbContext)
        {
            this.Logger = logger;
            this.Parser = parser;
            this.DownloadService = downloadService;
            this.Configuration = configuration;
            this.DrugDbContext = drugDbContext;
        }

        /// <summary>
        /// Gets or sets the file parser.
        /// </summary>
        protected T Parser { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        protected IConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the Downloader utility which gets the file and gives us a unique hash.
        /// </summary>
        protected IFileDownloadService DownloadService { get; set; }

        /// <summary>
        /// Gets or sets the database context to use to to interact with the DB.
        /// </summary>
        protected GatewayDbContext DrugDbContext { get; set; }

        /// <inheritdoc/>
        public virtual async Task ProcessAsync(string configSectionName, CancellationToken ct = default)
        {
            this.Logger.LogInformation("Reading configuration for section {ConfigSectionName}", configSectionName);
            IConfigurationSection section = this.Configuration.GetSection(configSectionName);
            Uri? source = section.GetValue<Uri>("Url");

            string? programType = section.GetValue<string>("AppName");
            this.Logger.LogInformation("Program Type = {ProgramType}", programType);
            string? targetFolder = this.Configuration.GetSection(configSectionName).GetValue<string>("TargetFolder");

            FileDownload downloadedFile = await this.DownloadFileAsync(source, targetFolder, ct);
            if (!await this.FileProcessedAsync(downloadedFile, ct))
            {
                string sourceFolder = this.ExtractFiles(downloadedFile);
                this.Logger.LogInformation("File has not been processed - Attempting to process");
                downloadedFile.ProgramCode = programType;
                await this.ProcessDownloadAsync(sourceFolder, downloadedFile, ct);
                this.RemoveExtractedFiles(sourceFolder);
            }
            else
            {
                this.Logger.LogInformation("File has been previously processed - exiting");
                if (downloadedFile.LocalFilePath != null && downloadedFile.Name != null)
                {
                    string filename = Path.Combine(downloadedFile.LocalFilePath, downloadedFile.Name);
                    this.Logger.LogInformation("Removing zip file: {Filename}", filename);
                    File.Delete(filename);
                }
                else
                {
                    this.Logger.LogWarning("Unable to clean up as FileDownload contains null data, LocalFilePath = {LocalFilePath} Name = {Name}", downloadedFile.LocalFilePath, downloadedFile.Name);
                }
            }
        }

        /// <summary>
        /// Processes the downloaded file.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="downloadedFile">The file download to process.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task ProcessDownloadAsync(string sourceFolder, FileDownload downloadedFile, CancellationToken ct = default);

        /// <summary>
        /// Adds the processed file to the DB to ensure we don't process again.
        /// </summary>
        /// <param name="downloadedFile">The FileDownload to add to the DB.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task AddFileToDbAsync(FileDownload downloadedFile, CancellationToken ct = default)
        {
            this.Logger.LogInformation("Marking file with hash {Hash} as processed in DB", downloadedFile.Hash);
            await this.DrugDbContext.FileDownload.AddAsync(downloadedFile, ct);
        }

        /// <summary>
        /// Removes All Download Files that match the Program type but do not match the file hash.
        /// </summary>
        /// <param name="downloadedFile">Search for all download files not matching this one.</param>
        protected virtual void RemoveOldFiles(FileDownload downloadedFile)
        {
            List<FileDownload> oldIds =
            [
                .. this.DrugDbContext.FileDownload
                    .Where(p => p.ProgramCode == downloadedFile.ProgramCode && p.Hash != downloadedFile.Hash)
                    .Select(f => new FileDownload { Id = f.Id, Version = f.Version }),
            ];
            oldIds.ForEach(s => this.Logger.LogInformation("Deleting old Download file with hash: {Hash}", s.Hash));
            this.DrugDbContext.RemoveRange(oldIds);
        }

        /// <summary>
        /// Downloads the given file to the target folder.
        /// </summary>
        /// <param name="source">The URI of a file to download.</param>
        /// <param name="targetFolder">The location to store the file.</param>
        /// <param name="ct">
        /// <see cref="CancellationToken"/> to manage the async request.
        /// </param>
        /// <returns>A FileDownload object.</returns>
        protected async Task<FileDownload> DownloadFileAsync(Uri source, string targetFolder, CancellationToken ct = default)
        {
            this.Logger.LogInformation("Downloading file from {Source} to {TargetFolder}", source, targetFolder);
            return await this.DownloadService.GetFileFromUrlAsync(source, targetFolder, true, ct);
        }

        /// <summary>
        /// Extracts the file referenced via FileDownload.
        /// </summary>
        /// <param name="downloadedFile">The FileDownload object to extract.</param>
        /// <returns>The path to the unzipped folder.</returns>
        protected string ExtractFiles(FileDownload downloadedFile)
        {
            if (downloadedFile is { LocalFilePath: not null, Name: not null })
            {
                string filename = Path.Combine(downloadedFile.LocalFilePath, downloadedFile.Name);
                this.Logger.LogInformation("Extracting zip file: {Filename}", filename);
                string unzippedPath = Path.Combine(downloadedFile.LocalFilePath, Path.GetFileNameWithoutExtension(downloadedFile.Name));
                ZipFile.ExtractToDirectory(filename, unzippedPath);
                this.Logger.LogInformation("Deleting Zip file");
                File.Delete(filename);
                return unzippedPath;
            }

            throw new ArgumentNullException(
                nameof(downloadedFile),
                $"Downloaded file has null attributes, LocalFilePath = {downloadedFile.LocalFilePath} Name = {downloadedFile.Name}");
        }

        /// <summary>
        /// Recursively deletes the files at folder.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        protected void RemoveExtractedFiles(string folder)
        {
            this.Logger.LogInformation("Removing extracted files under {Folder}", folder);
            Directory.Delete(folder, true);
        }

        /// <summary>
        /// Confirms if the downloadedFile has been processed previously.
        /// </summary>
        /// <param name="downloadedFile">The file to verify.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>True if the file has been previously processed.</returns>
        protected async Task<bool> FileProcessedAsync(FileDownload downloadedFile, CancellationToken ct = default)
        {
            return await this.DrugDbContext.FileDownload.AnyAsync(p => p.Hash == downloadedFile.Hash, ct);
        }
    }
}
