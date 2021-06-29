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
namespace HealthGateway.WebClient.Services
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Delegates;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class ReportService : IReportService
    {
        private readonly ILogger logger;
        private readonly ICDogsDelegate cdogsDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="cdogsDelegate">Injected CDOGS delegate.</param>
        public ReportService(ILogger<ReportService> logger, ICDogsDelegate cdogsDelegate)
        {
            this.logger = logger;
            this.cdogsDelegate = cdogsDelegate;
        }

        /// <inheritdoc />
        public RequestResult<ReportModel> GetReport(ReportRequestModel reportRequest)
        {
            this.logger.LogTrace($"New report request: {JsonSerializer.Serialize(reportRequest)}");

            CDogsRequestModel cdogsRequest = new ()
            {
                Data = reportRequest.Data,
                Options = new CDogsOptionsModel()
                {
                    ConvertTo = reportRequest.Type.ToString().ToLower(),
                    ReportName = $"HealthGateway{reportRequest.Template}Report",
                },
                Template = new CDogsTemplateModel()
                {
                    Content = this.ReadTemplate(reportRequest.Template),
                },
            };

            RequestResult<ReportModel> retVal = Task.Run(async () => await this.cdogsDelegate.GenerateReportAsync(cdogsRequest).ConfigureAwait(true)).Result;
            this.logger.LogTrace($"Finished generating report: {JsonSerializer.Serialize(retVal)}");
            return retVal;
        }

        private string ReadTemplate(TemplateType template)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(ReportService));
            Stream? resourceStream = assembly!.GetManifestResourceStream($"HealthGateway.WebClient.Server.Assets.Templates.{template}Report.docx");

            if (resourceStream == null)
            {
                throw new FileNotFoundException("Template not found");
            }

            using MemoryStream memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }
}
