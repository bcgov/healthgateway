using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using HealthGateway.Engine.Core;
using HealthGateway.Engine.Extensions;

namespace HealthGateway.Engine.FhirResponseFactory
{
    public interface IFhirResponseFactory
    {
        ServerFhirResponse GetFhirResponse(Entry entry, IKey key = null, IEnumerable<object> parameters = null);
        ServerFhirResponse GetFhirResponse(Entry entry, IKey key = null, params object[] parameters);
        ServerFhirResponse GetMetadataResponse(Entry entry, IKey key = null);
        ServerFhirResponse GetFhirResponse(IList<Entry> interactions, Bundle.BundleType bundleType);
        ServerFhirResponse GetFhirResponse(Bundle bundle);
        ServerFhirResponse GetFhirResponse(IEnumerable<Tuple<Entry, ServerFhirResponse>> responses, Bundle.BundleType bundleType);
    }

    public class FhirResponseFactory : IFhirResponseFactory
    {

        public ServerFhirResponse GetFhirResponse(Entry entry, IKey key = null, IEnumerable<object> parameters = null)
        {
            if (entry == null)
            {
                return Respond.NotFound(key);
            }
            if (entry.IsDeleted())
            {
                return Respond.Gone(entry);
            }

            ServerFhirResponse response = null;

            return response ?? Respond.WithResource(entry);
        }

        public ServerFhirResponse GetFhirResponse(Entry entry, IKey key = null, params object[] parameters)
        {
            return GetFhirResponse(entry, key, parameters.ToList());
        }

        public ServerFhirResponse GetMetadataResponse(Entry entry, IKey key = null)
        {
            if (entry == null)
            {
                return Respond.NotFound(key);
            }
            else if (entry.IsDeleted())
            {
                return Respond.Gone(entry);
            }

            return Respond.WithMeta(entry);
        }

        public ServerFhirResponse GetFhirResponse(IList<Entry> interactions, Bundle.BundleType bundleType)
        {
            //Bundle bundle = localhost.CreateBundle(bundleType).Append(interactions);
            Bundle bundle = new Bundle().Append(interactions);
            return Respond.WithBundle(bundle);
        }

        public ServerFhirResponse GetFhirResponse(IEnumerable<Tuple<Entry, ServerFhirResponse>> responses, Bundle.BundleType bundleType)
        {
            //Bundle bundle = localhost.CreateBundle(bundleType);
            Bundle bundle = new Bundle() { Type = bundleType };

            foreach (Tuple<Entry, ServerFhirResponse> response in responses)
            {
                bundle.Append(response.Item1, response.Item2);
            }

            return Respond.WithBundle(bundle);
        }

        public ServerFhirResponse GetFhirResponse(Bundle bundle)
        {
            return Respond.WithBundle(bundle);
        }
    }
}