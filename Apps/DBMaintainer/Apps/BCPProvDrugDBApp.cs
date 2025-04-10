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
namespace HealthGateway.DBMaintainer.Apps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Models;
    using HealthGateway.DBMaintainer.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility program to load the BC Government PharmaCare drug file.
    /// See
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files
    /// for reference.
    /// </summary>
    public class BcpProvDrugDbApp : BaseDrugApp<IPharmaCareDrugParser>
    {
        private readonly IPharmacyAssessmentParser pharmacyAssessmentParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="BcpProvDrugDbApp"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="parser">The parser to use.</param>
        /// <param name="downloadService">The Download Service Utility.</param>
        /// <param name="configuration">The Configuration.</param>
        /// <param name="drugDbContext">The database context.</param>
        /// <param name="pharmacyAssessmentParser">The pharmacy assessment parser to use.</param>
        protected BcpProvDrugDbApp(
            ILogger<BcpProvDrugDbApp> logger,
            IPharmaCareDrugParser parser,
            IFileDownloadService downloadService,
            IConfiguration configuration,
            GatewayDbContext drugDbContext,
            IPharmacyAssessmentParser pharmacyAssessmentParser)
            : base(logger, parser, downloadService, configuration, drugDbContext)
        {
            this.pharmacyAssessmentParser = pharmacyAssessmentParser;
        }

        /// <inheritdoc/>
        public override async Task ProcessAsync(string configSectionName, CancellationToken ct = default)
        {
            this.Logger.LogInformation("Reading configuration for section {ConfigSectionName}", configSectionName);

            FileDownload pharmaCareDrugFileDownload = await this.GetFileDownloadAsync(this.Configuration.GetSection(configSectionName), ct);
            FileDownload pharmacyAssessmentFileDownload = await this.GetFileDownloadAsync(this.Configuration.GetSection("PharmacyAssessmentFile"), ct);

            bool isPharmaCareDrugFileProcessed = await this.FileProcessedAsync(pharmaCareDrugFileDownload, ct);
            bool isPharmacyAssessmentFileProcessed = await this.FileProcessedAsync(pharmacyAssessmentFileDownload, ct);

            this.Logger.LogInformation(
                "Pharma care drug file processed: {PharmaCareDrugFileProcessed} - Pharmacy assessment file processed: {PharmacyAssessmentFileProcessed}",
                isPharmaCareDrugFileProcessed,
                isPharmacyAssessmentFileProcessed);

            if (!isPharmaCareDrugFileProcessed || !isPharmacyAssessmentFileProcessed)
            {
                this.Logger.LogInformation("Pharma care drug file and/or pharmacy assessment file have not been processed - Attempting to process");

                // Unzip the file
                string pharmaCareDrugSourceFolder = this.ExtractFiles(pharmaCareDrugFileDownload);

                // Load the pharma care drug file
                await this.ProcessDownloadAsync(pharmaCareDrugSourceFolder, pharmaCareDrugFileDownload, pharmacyAssessmentFileDownload, ct);

                // Delete downloaded files
                this.RemoveExtractedFiles(pharmaCareDrugSourceFolder);
            }
            else
            {
                this.Logger.LogInformation("Files have already been previously processed - exiting");
                this.DeleteDownloadedFiles(pharmaCareDrugFileDownload);
                this.DeleteDownloadedFiles(pharmacyAssessmentFileDownload);
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessDownloadAsync(string sourceFolder, FileDownload downloadedFile, CancellationToken ct = default)
        {
            string[] files = Directory.GetFiles(sourceFolder, "pddf*.csv");
            if (files.Length > 1)
            {
                throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.");
            }

            this.Logger.LogInformation("Parsing Provincial PharmaCare file");
            this.RemoveOldFiles(downloadedFile);
            await this.AddFileToDbAsync(downloadedFile, ct);
            await this.DrugDbContext.AddRangeAsync(this.Parser.ParsePharmaCareDrugFileAsync(files[0], downloadedFile, ct), ct);
            this.Logger.LogInformation("Saving PharmaCare Drugs");
            await this.DrugDbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Deletes all file downloads that match the program code.
        /// </summary>
        /// <param name="downloadedFile">Search for all download files matching this one.</param>
        protected override void RemoveOldFiles(FileDownload downloadedFile)
        {
            List<FileDownload> oldIds =
            [
                .. this.DrugDbContext.FileDownload
                    .Where(p => p.ProgramCode == downloadedFile.ProgramCode)
                    .Select(f => new FileDownload { Id = f.Id, ProgramCode = f.ProgramCode, Version = f.Version }),
            ];
            oldIds.ForEach(s => this.Logger.LogInformation("Deleting old file downloads with id: {Id} and program code: {ProgramCode}", s.Id, s.ProgramCode));
            this.DrugDbContext.RemoveRange(oldIds);
        }

        private static string GetDownloadedFile(string sourceFolder, string filenamePattern)
        {
            string[] files = Directory.GetFiles(sourceFolder, filenamePattern);
            return files.Length > 1 ? throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.") : files[0];
        }

        private static void ModifyPharmaCareDrug(PharmacyAssessment pharmacyAssessment, IList<PharmaCareDrug> pharmaCareDrugs)
        {
            foreach (PharmaCareDrug pharmaCareDrug in pharmaCareDrugs.Where(pd => pd.DinPin == pharmacyAssessment.Pin))
            {
                pharmaCareDrug.PharmacyAssessmentTitle = pharmacyAssessment.PharmacyAssessmentTitle;
                pharmaCareDrug.PrescriptionProvided = pharmacyAssessment.PrescriptionProvided;
                pharmaCareDrug.RedirectedToHealthCareProvider = pharmacyAssessment.RedirectedToHealthCareProvider;
            }
        }

        private void DeleteDownloadedFiles(FileDownload fileDownload)
        {
            if (fileDownload is { LocalFilePath: not null, Name: not null })
            {
                string filename = Path.Combine(fileDownload.LocalFilePath, fileDownload.Name);
                this.Logger.LogInformation("Removing zip file: {Filename}", filename);
                File.Delete(filename);
            }
            else
            {
                this.Logger.LogWarning(
                    "Unable to clean up as FileDownload contains null data, LocalFilePath = {LocalFilePath} Name = {Name}",
                    fileDownload.LocalFilePath,
                    fileDownload.Name);
            }
        }

        private async Task<FileDownload> GetFileDownloadAsync(IConfigurationSection section, CancellationToken ct)
        {
            Uri? file = section.GetValue<Uri>("Url");
            string? targetFolder = section.GetValue<string>("TargetFolder");
            FileDownload fileDownload = await this.DownloadFileAsync(file, targetFolder, ct);
            fileDownload.ProgramCode = section.GetValue<string>("AppName");
            this.Logger.LogInformation("Downloaded file for program code: {ProgramCode}", fileDownload.ProgramCode);
            return fileDownload;
        }

        private async Task ProcessDownloadAsync(string pharmaCareDrugSourceFolder, FileDownload pharmaCareDrugFileDownload, FileDownload pharmacyAssessmentFileDownload, CancellationToken ct)
        {
            string pharmaCareDrugFile = GetDownloadedFile(pharmaCareDrugSourceFolder, "pddf*.csv");
            string pharmacyAssessmentFile = GetDownloadedFile(pharmacyAssessmentFileDownload.LocalFilePath, pharmacyAssessmentFileDownload.Name);

            // Remove old pharma care drug file download in database
            this.RemoveOldFiles(pharmaCareDrugFileDownload);

            // Add new pharma care drug file download to database
            await this.AddFileToDbAsync(pharmaCareDrugFileDownload, ct);

            // Remove old pharmacy assessment file download in database
            this.RemoveOldFiles(pharmacyAssessmentFileDownload);

            // Add new pharmacy assessment file download to database
            await this.AddFileToDbAsync(pharmacyAssessmentFileDownload, ct);

            // Parse Provincial PharmaCare Drug file
            IList<PharmaCareDrug> pharmaCareDrugs = await this.Parser.ParsePharmaCareDrugFileAsync(pharmaCareDrugFile, pharmaCareDrugFileDownload, ct);

            // Parse Pharmacy Assessment file
            IEnumerable<PharmacyAssessment> pharmacyAssessments = this.pharmacyAssessmentParser.ParsePharmacyAssessmentFile(pharmacyAssessmentFile, pharmacyAssessmentFileDownload);

            // If required, modify pharma care drug with pharmacy assessment
            pharmacyAssessments.ToList().ForEach(pa => ModifyPharmaCareDrug(pa, pharmaCareDrugs));

            await this.DrugDbContext.AddRangeAsync(pharmaCareDrugs, ct);
            this.Logger.LogInformation("Saving pharma care drugs to database");
            await this.DrugDbContext.SaveChangesAsync(ct);
        }
    }
}
