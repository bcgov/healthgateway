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
        public override void Process(string configSectionName)
        {
            this.Logger.LogInformation("Reading configuration for section {ConfigSectionName}", configSectionName);

            FileDownload pharmaCareDrugFileDownload = this.GetFileDownload(this.Configuration.GetSection(configSectionName));
            FileDownload pharmacyAssessmentFileDownload = this.GetFileDownload(this.Configuration.GetSection("PharmacyAssessmentFile"));

            bool isPharmaCareDrugFileProcessed = this.FileProcessed(pharmaCareDrugFileDownload);
            bool isPharmacyAssessmentFileProcessed = this.FileProcessed(pharmacyAssessmentFileDownload);

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
                this.ProcessDownload(pharmaCareDrugSourceFolder, pharmaCareDrugFileDownload, pharmacyAssessmentFileDownload);

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
        protected override void ProcessDownload(string sourceFolder, FileDownload downloadedFile)
        {
            string[] files = Directory.GetFiles(sourceFolder, "pddf*.csv");
            if (files.Length > 1)
            {
                throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.");
            }

            this.Logger.LogInformation("Parsing Provincial PharmaCare file");
            this.RemoveOldFiles(downloadedFile);
            this.AddFileToDb(downloadedFile);
            this.DrugDbContext.AddRange(this.Parser.ParsePharmaCareDrugFile(files[0], downloadedFile));
            this.Logger.LogInformation("Saving PharmaCare Drugs");
            this.DrugDbContext.SaveChanges();
        }

        /// <summary>
        /// Deletes all file downloads that match the program code.
        /// </summary>
        /// <param name="downloadedFile">Search for all download files matching this one.</param>
        protected override void RemoveOldFiles(FileDownload downloadedFile)
        {
            List<FileDownload> oldIds = this.DrugDbContext.FileDownload
                .Where(p => p.ProgramCode == downloadedFile.ProgramCode)
                .Select(f => new FileDownload { Id = f.Id, ProgramCode = f.ProgramCode, Version = f.Version })
                .ToList();
            oldIds.ForEach(s => this.Logger.LogInformation("Deleting old file downloads with id: {Id} and program code: {ProgramCode}", s.Id, s.ProgramCode));
            this.DrugDbContext.RemoveRange(oldIds);
        }

        private static string GetDownloadedFile(string sourceFolder, string filenamePattern)
        {
            string[] files = Directory.GetFiles(sourceFolder, filenamePattern);
            if (files.Length > 1)
            {
                throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.");
            }

            return files[0];
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
            if (fileDownload.LocalFilePath != null && fileDownload.Name != null)
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

        private FileDownload GetFileDownload(IConfigurationSection section)
        {
            Uri? file = section.GetValue<Uri>("Url");
            string? targetFolder = section.GetValue<string>("TargetFolder");
            FileDownload fileDownload = this.DownloadFile(file, targetFolder);
            fileDownload.ProgramCode = section.GetValue<string>("AppName");
            this.Logger.LogInformation("Downloaded file for program code: {ProgramCode}", fileDownload.ProgramCode);
            return fileDownload;
        }

        private void ProcessDownload(string pharmaCareDrugSourceFolder, FileDownload pharmaCareDrugFileDownload, FileDownload pharmacyAssessmentFileDownload)
        {
            string pharmaCareDrugFile = GetDownloadedFile(pharmaCareDrugSourceFolder, "pddf*.csv");
            string pharmacyAssessmentFile = GetDownloadedFile(pharmacyAssessmentFileDownload.LocalFilePath, pharmacyAssessmentFileDownload.Name);

            // Remove old pharma care drug file download in database
            this.RemoveOldFiles(pharmaCareDrugFileDownload);

            // Add new pharma care drug file download to database
            this.AddFileToDb(pharmaCareDrugFileDownload);

            // Remove old pharmacy assessment file download in database
            this.RemoveOldFiles(pharmacyAssessmentFileDownload);

            // Add new pharmacy assessment file download to database
            this.AddFileToDb(pharmacyAssessmentFileDownload);

            this.Logger.LogInformation("Parsing Provincial PharmaCare Drug file");
            IList<PharmaCareDrug> pharmaCareDrugs = this.Parser.ParsePharmaCareDrugFile(pharmaCareDrugFile, pharmaCareDrugFileDownload);

            this.Logger.LogInformation("Parsing Pharmacy Assessment file");
            IEnumerable<PharmacyAssessment> pharmacyAssessments = this.pharmacyAssessmentParser.ParsePharmacyAssessmentFile(pharmacyAssessmentFile, pharmacyAssessmentFileDownload);

            // If required, modify pharma care drug with pharmacy assessment
            pharmacyAssessments.ToList().ForEach(pa => ModifyPharmaCareDrug(pa, pharmaCareDrugs));

            this.DrugDbContext.AddRange(pharmaCareDrugs);
            this.Logger.LogInformation("Saving pharma care drugs to database");
            this.DrugDbContext.SaveChanges();
        }
    }
}
