using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HealthGateway.Engine.Core
{
    public class ConditionalHeaderParameters
    {
        public IEnumerable<string> IfNoneMatchTags { get; set; }
        public DateTimeOffset? IfModifiedSince { get; set; }

        public ConditionalHeaderParameters()
        {
        }

        public ConditionalHeaderParameters(HttpRequest request)
        {
            IfNoneMatchTags = FhirHttpUtil.IfNoneMatch(request);
            IfModifiedSince = FhirHttpUtil.IfModifiedSince(request);
        }
    }
}