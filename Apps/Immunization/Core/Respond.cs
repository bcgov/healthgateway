using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Net;
using HealthGateway.Engine.Extensions;

namespace HealthGateway.Engine.Core
{
    // This class serves instances of "Response"
    public static class Respond
    {
        public static ServerFhirResponse WithError(HttpStatusCode code)
        {
            return new ServerFhirResponse(code, Key.Null, null);
        }

        public static ServerFhirResponse WithCode(HttpStatusCode code)
        {
            return new ServerFhirResponse(code, null);
        }

        public static ServerFhirResponse WithCode(int code)
        {
            return new ServerFhirResponse((HttpStatusCode)code, null);
        }

        public static ServerFhirResponse WithError(HttpStatusCode code, string message, params object[] args)
        {
            OperationOutcome outcome = new OperationOutcome();
            outcome.AddError(string.Format(message, args));
            return new ServerFhirResponse(code, outcome);
        }

        public static ServerFhirResponse WithResource(int code, Resource resource)
        {
            return new ServerFhirResponse((HttpStatusCode)code, resource);
        }

        public static ServerFhirResponse WithResource(Resource resource)
        {
            return new ServerFhirResponse(HttpStatusCode.OK, resource);
        }

        public static ServerFhirResponse WithResource(Key key, Resource resource)
        {
            return new ServerFhirResponse(HttpStatusCode.OK, key, resource);
        }

        public static ServerFhirResponse WithResource(HttpStatusCode code, Key key, Resource resource)
        {
            return new ServerFhirResponse(code, key, resource);
        }

        public static ServerFhirResponse WithEntry(HttpStatusCode code, Entry entry)
        {

            return new ServerFhirResponse(code, entry.Key, entry.Resource);
        }

        public static ServerFhirResponse WithBundle(Bundle bundle)
        {
            return new ServerFhirResponse(HttpStatusCode.OK, bundle);
        }

        public static ServerFhirResponse WithBundle(IEnumerable<Entry> entries, Uri serviceBase)
        {
            Bundle bundle = new Bundle();
            bundle.Append(entries);
            return WithBundle(bundle);
        }

        public static ServerFhirResponse WithMeta(Meta meta)
        {
            Parameters parameters = new Parameters();
            parameters.Add(typeof(Meta).Name, meta);
            return Respond.WithResource(parameters);
        }

        public static ServerFhirResponse WithMeta(Entry entry)
        {
            if (entry.Resource != null && entry.Resource.Meta != null)
            {
                return Respond.WithMeta(entry.Resource.Meta);
            }
            else
            {
                return Respond.WithError(HttpStatusCode.InternalServerError, "Could not retrieve meta. Meta was not present on the resource");
            }
        }

        public static ServerFhirResponse WithKey(HttpStatusCode code, IKey key)
        {
            return new ServerFhirResponse(code, key, null);
        }

        public static ServerFhirResponse WithResource(HttpStatusCode code, Entry entry)
        {
            return new ServerFhirResponse(code, entry.Key, entry.Resource);
        }

        public static ServerFhirResponse WithResource(Entry entry)
        {
            return new ServerFhirResponse(HttpStatusCode.OK, entry.Key, entry.Resource);
        }

        public static ServerFhirResponse NotFound(IKey key)
        {
            if (key.VersionId == null)
            {
                return Respond.WithError(HttpStatusCode.NotFound, "No {0} resource with id {1} was found.", key.TypeName, key.ResourceId);
            }
            else
            {
                return Respond.WithError(HttpStatusCode.NotFound, "There is no {0} resource with id {1}, or there is no version {2}", key.TypeName, key.ResourceId, key.VersionId);
            }
            // For security reasons (leakage): keep message in sync with Error.NotFound(key)
        }

        public static ServerFhirResponse Gone(Entry entry)
        {

            var message = String.Format(
                  "A {0} resource with id {1} existed, but was deleted on {2} (version {3}).",
                  entry.Key.TypeName,
                  entry.Key.ResourceId,
                  entry.When,
                  entry.Key.ToRelativeUri());

            return Respond.WithError(HttpStatusCode.Gone, message);
        }

        public static ServerFhirResponse NotImplemented
        {
            get
            {
                return Respond.WithError(HttpStatusCode.NotImplemented);
            }
        }

        public static ServerFhirResponse Success
        {
            get
            {
                return new ServerFhirResponse(HttpStatusCode.OK);
            }
        }
    }
}
