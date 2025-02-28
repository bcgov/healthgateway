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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    public class ReportService : IReportService
    {
        private readonly ICDogsDelegate cdogsDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="cdogsDelegate">Injected CDOGS delegate.</param>
        public ReportService(ICDogsDelegate cdogsDelegate)
        {
            this.cdogsDelegate = cdogsDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GetReportAsync(ReportRequestModel reportRequest, CancellationToken ct = default)
        {
            string reportName = $"HealthGateway{reportRequest.Template}Report";
            CDogsRequestModel cdogsRequest = new()
            {
                Data = reportRequest.Data,
                Options = new CDogsOptionsModel
                {
                    Overwrite = true,
                    ConvertTo = reportRequest.Type.ToString().ToLower(CultureInfo.CurrentCulture),
                    ReportName = reportName,
                },
                Template = new CDogsTemplateModel
                {
                    Content = ReadTemplate(reportRequest.Template, reportRequest.Type),
                    FileType = GetTemplateExtension(reportRequest.Type),
                },
            };

            return await this.cdogsDelegate.GenerateReportAsync(cdogsRequest, ct);
        }

        private static string GetTemplateExtension(ReportFormatType formatType)
        {
            return formatType switch
            {
                ReportFormatType.Pdf => "docx",
                ReportFormatType.Xlsx or ReportFormatType.Csv => "xlsx",
                _ => string.Empty,
            };
        }

        private static string ReadTemplate(TemplateType template, ReportFormatType formatType)
        {
            string extension = GetTemplateExtension(formatType);
            string resourceName = $"HealthGateway.GatewayApi.Assets.Templates.{template}Report.{extension}";

            Assembly? assembly = Assembly.GetAssembly(typeof(ReportService));
            Stream resourceStream = assembly!.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Template {resourceName} not found.");
            using MemoryStream memoryStream = new();
            resourceStream.CopyTo(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }
}
