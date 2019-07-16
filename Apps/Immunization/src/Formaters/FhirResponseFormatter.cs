using System;
using System.IO;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HealthGateway.Formatters
{
    public class FhirResponseFormatter : OutputFormatter
    {
        private readonly FhirJsonSerializer jsonSerializer = new FhirJsonSerializer();

        public FhirResponseFormatter()
        {
            this.SupportedMediaTypes.Clear();
            //Look for specific media type declared with Content-Type header in request  
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue(ContentType.JSON_CONTENT_HEADER));
        }

        public override async System.Threading.Tasks.Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            using (StringWriter strWriter = new StringWriter())
            using (JsonWriter writer = new JsonTextWriter(strWriter))
            {
                SummaryType summary = SummaryType.False;//;requestMessage.RequestSummary();

                Type type = context.Object.GetType();
                if (type == typeof(OperationOutcome))
                {
                    Resource resource = context.Object as Resource;
                    jsonSerializer.Serialize(resource, writer, summary);
                }
                else if (typeof(Resource).IsAssignableFrom(type))
                {
                    Resource resource = context.Object as Resource;
                    jsonSerializer.Serialize(resource, writer, summary);
                }
                else if (typeof(ServerFhirResponse).IsAssignableFrom(type))
                {
                    ServerFhirResponse fhirResponse = (context.Object as ServerFhirResponse);
                    if (fhirResponse.HasBody)
                    {
                        jsonSerializer.Serialize(fhirResponse.Resource, writer, summary);
                    }
                }

                HttpResponse response = context.HttpContext.Response;
                await response.WriteAsync(strWriter.ToString());
            }
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(OperationOutcome).IsAssignableFrom(type) ||
                typeof(Resource).IsAssignableFrom(type) ||
                typeof(ServerFhirResponse).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }
    }
}
