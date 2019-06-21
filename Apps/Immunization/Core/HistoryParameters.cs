using System;
using Microsoft.AspNetCore.Http;

namespace HealthGateway.Engine.Core
{
    public class HistoryParameters
    {
        public HistoryParameters()
        {
        }

        public HistoryParameters(HttpRequest request)
        {
            Count = FhirHttpUtil.GetIntParameter(request, FhirParameter.COUNT);
            Since = FhirHttpUtil.GetDateParameter(request, FhirParameter.SINCE);
            SortBy = FhirHttpUtil.GetStringParameter(request, FhirParameter.SORT);
        }

        public int? Count { get; set; }

        public DateTimeOffset? Since { get; set; }

        public string Format { get; set; }

        public string SortBy { get; set; }
    }
}