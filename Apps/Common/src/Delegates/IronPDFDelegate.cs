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

    /// <inheritdoc/>
    public class IronPDFDelegate : IIronPDFDelegate
    {
        private static ActivitySource Source { get; } = new ActivitySource(nameof(IronPDFDelegate));

        /// <inheritdoc/>
        public RequestResult<ReportModel> GeneratePDF(IronPDFRequestModel request)
        {
            string html = request.HtmlTemplate;
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
                PdfDocument pdfDoc = renderer.RenderHtmlAsPdf(html);

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
    }
}
