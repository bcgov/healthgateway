using Hl7.Fhir.Model;
using System.Net;

namespace HealthGateway.Engine.Core
{
    public class ServerFhirResponse
    {
        public readonly HttpStatusCode StatusCode;
        public readonly IKey Key;
        public readonly Resource Resource;

        public ServerFhirResponse(HttpStatusCode code, IKey key, Resource resource)
        {
            this.StatusCode = code;
            this.Key = key;
            this.Resource = resource;
        }

        public ServerFhirResponse(HttpStatusCode code, Resource resource)
        {
            this.StatusCode = code;
            this.Key = null;
            this.Resource = resource;
        }

        public ServerFhirResponse(HttpStatusCode code)
        {
            this.StatusCode = code;
            this.Key = null;
            this.Resource = null;
        }

        public bool IsValid
        {
            get
            {
                int code = (int)this.StatusCode;
                return code <= 300;
            }
        }

        public bool HasBody
        {
            get
            {
                return Resource != null;
            }
        }

        public override string ToString()
        {
            string details = (Resource != null) ? string.Format("({0})", Resource.TypeName) : null;
            string location = Key?.ToString();
            return string.Format("{0}: {1} {2} ({3})", (int)StatusCode, StatusCode.ToString(), details, location);
        }
    }
}