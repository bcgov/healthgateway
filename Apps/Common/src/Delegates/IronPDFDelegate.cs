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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Diagnostics;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using IronPdf;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class IronPDFDelegate : IIronPDFDelegate
    {
        private const string ConfigKey = "IronPDF";
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IronPDFDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger to use.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public IronPDFDelegate(ILogger<IronPDFDelegate> logger, IConfiguration configuration)
        {
            this.logger = logger;
            configuration.Bind(ConfigKey, this.Configuration);
            InitializeLicense(logger, this.Configuration);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(IronPDFDelegate));

        private IronPDFConfig Configuration { get; set; } = new ();

        /// <inheritdoc/>
        public RequestResult<ReportModel> GeneratePDF(IronPDFRequestModel request)
        {
            string html = request.HtmlTemplate;
            this.logger.LogTrace("Generating PDF");
            using (Source.StartActivity("GeneratePDF"))
            {
                if (request.Data != null)
                {
                    foreach (var dataItem in request.Data)
                    {
                        html = html.Replace($"{{{dataItem.Key}}}", dataItem.Value, StringComparison.InvariantCulture);
                    }
                }

                using HtmlToPdf renderer = new HtmlToPdf();
                renderer.RenderingOptions.PrintHtmlBackgrounds = true;
                PdfDocument pdfDoc = renderer.RenderHtmlAsPdf(html);

                this.logger.LogTrace("Applying metadata to PDF");
                pdfDoc.SecuritySettings.AllowUserAnnotations = this.Configuration.AllowUserAnnotations;
                if (this.Configuration.AllowUserEdits)
                {
                    pdfDoc.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
                }

                pdfDoc.MetaData.Producer = this.Configuration.Producer;
                pdfDoc.MetaData.Author = this.Configuration.Author;
                pdfDoc.MetaData.Subject = this.Configuration.Subject;
                pdfDoc.MetaData.Title = this.Configuration.Title;

                return new RequestResult<ReportModel>()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new ReportModel()
                    {
                        Data = Convert.ToBase64String(pdfDoc.Stream.ToArray()),
                        FileName = request.FileName,
                    },
                };
            }
        }

        private static void InitializeLicense(ILogger logger, IronPDFConfig config)
        {
            if (!License.IsLicensed)
            {
                if (!string.IsNullOrEmpty(config.LicenseKey))
                {
                    logger.LogInformation("Applying IronPdf license");
                    License.LicenseKey = config.LicenseKey;
                    if (!License.IsLicensed)
                    {
                        logger.LogWarning("IronPdf is invalid and will result in watermarks");
                    }
                }
                else
                {
                    logger.LogWarning("IronPdf license was not configured and will result in watermarks");
                }
            }
        }
    }
}
