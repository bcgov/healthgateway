// -------------------------------------------------------------------------
//  Copyright © 2019-2020 Province of British Columbia
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
namespace HealthGateway.DrugMaintainer.Apps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The base class for HealthGateway Drug file loading utility programs.
    /// </summary>
    /// <typeparam name="T">The parser to use to process files.</typeparam>
    public abstract class BaseDrugApp<T> : IDrugApp
    {
        /// <summary>
        /// Gets or sets the file parser.
        /// </summary>
        protected T parser { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        protected ILogger logger { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        protected IConfiguration configuration { get; set; }

        /// <summary>
        /// The Downloader utility which gets the file and gives us a unique hash.
        /// </summary>
        protected IFileDownloadService downloadService { get; set; }

        /// <summary>
        /// The database contect to use to to interact with the DB.
        /// </summary>
        protected GatewayDbContext drugDbContext { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDrugApp{T}"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="parser">The file parser.</param>
        /// <param name="downloadService">The download utility.</param>
        /// <param name="configuration">The IConfiguration to use.</param>
        /// <param name="drugDBContext">The database context to interact with./</param>
        public BaseDrugApp(ILogger logger, T parser, IFileDownloadService downloadService, IConfiguration configuration, GatewayDbContext drugDBContext)
        {
            this.logger = logger;
            this.parser = parser;
            this.downloadService = downloadService;
            this.configuration = configuration;
            this.drugDbContext = drugDBContext;
        }

        /// <summary>
        /// Downloads the given file to the target folder.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        private FileDownload DownloadFile(Uri source, string targetFolder)
        {
            logger.LogInformation($"Downloading file from {source.ToString()} to {targetFolder}");
            return Task.Run(async() => await this.downloadService.GetFileFromUrl(source, targetFolder, true)).Result;
        }

        /// <summary>
        /// Extracts the file referenced via FileDownload.
        /// </summary>
        /// <param name="downloadedFile"></param>
        /// <returns>The path to the unzipped folder.</returns>
        private string ExtractFiles(FileDownload downloadedFile)
        {
            string filename = Path.Combine(downloadedFile.LocalFilePath, downloadedFile.Name);
            logger.LogInformation($"Extracting zip file: {filename}");
            string unzipedPath = Path.Combine(downloadedFile.LocalFilePath, Path.GetFileNameWithoutExtension(downloadedFile.Name));
            ZipFile.ExtractToDirectory(filename, unzipedPath);
            logger.LogInformation("Deleting Zip file");
            File.Delete(filename);
            return unzipedPath;
        }

        /// <summary>
        /// Recursively deletes the files at folder.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        private void RemoveExtractedFiles(string folder)
        {
            logger.LogInformation($"Removing extracted files under {folder}");
            Directory.Delete(folder, true);
        }

        /// <summary>
        /// Confirms if the downloadedFile has been processed previously.
        /// </summary>
        /// <param name="downloadedFile">The file to verify.</param>
        /// <returns></returns>
        private bool FileProcessed(FileDownload downloadedFile)
        {
            return this.drugDbContext.FileDownload.Where(p => p.Hash == downloadedFile.Hash).Any();
        }

        /// <summary>
        /// Adds the processed file to the DB to ensure we don't process again.
        /// </summary>
        /// <param name="downloadedFile"></param>
        protected void AddFileToDB(FileDownload downloadedFile)
        {
            logger.LogInformation($"Marking file with hash {downloadedFile.Hash} as processed in DB");
            this.drugDbContext.FileDownload.Add(downloadedFile);
        }

        /// <summary>
        /// Removes All Download Files that match the Program type but do not match the file hash. 
        /// </summary>
        /// <param name="downloadedFile">Search for all download files not matching this one.</param>
        protected void RemoveOldFiles(FileDownload downloadedFile)
        {
            List<FileDownload> oldIds = this.drugDbContext.FileDownload
                                            .Where(p => p.ProgramCode == downloadedFile.ProgramCode && p.Hash != downloadedFile.Hash)
                                            .Select(f => new FileDownload{ Id = f.Id, Version = f.Version})
                                            .ToList();
            oldIds.ForEach(s => logger.LogInformation($"Deleting old Download file with hash: {s}"));
            this.drugDbContext.RemoveRange(oldIds);
        }

        /// <summary>
        /// Performs the actual download of the file and verifies if it has been
        /// previously processed.
        /// </summary>
        public virtual void Process(string configSectionName)
        {
            this.logger.LogInformation($"Reading configuration for section {configSectionName}");
            IConfigurationSection section = configuration.GetSection(configSectionName);
            Uri source = section.GetValue<Uri>("Url");

            string programType = section.GetValue<string>("AppName");
            this.logger.LogInformation($"Program Type = {programType}");
            string targetFolder = configuration.GetSection(configSectionName).GetValue<string>("TargetFolder");

            FileDownload downloadedFile = DownloadFile(source, targetFolder);
            if (!FileProcessed(downloadedFile))
            {
                string sourceFolder = ExtractFiles(downloadedFile);
                logger.LogInformation("File has not been processed - Attempting to process");
                downloadedFile.ProgramCode = programType;
                ProcessDownload(sourceFolder, downloadedFile);
                RemoveExtractedFiles(sourceFolder);
            }
            else
            {
                this.logger.LogInformation("File has been previously processed - exiting");
                string filename = Path.Combine(downloadedFile.LocalFilePath, downloadedFile.Name);
                logger.LogInformation($"Removing zip file: {filename}");
                File.Delete(filename);
            }
        }

        /// <summary>
        /// Processes the downloaded file.
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="downloadedFile"></param>
        public abstract void ProcessDownload(string sourceFolder, FileDownload downloadedFile);
    }
}
