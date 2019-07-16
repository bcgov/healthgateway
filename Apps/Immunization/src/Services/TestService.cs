using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthGateway.Engine.Core;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.FhirResponseFactory;
using HealthGateway.Engine.Extensions;

namespace HealthGateway.Service
{
    public class TestService : IFhirService
    {
        private readonly IFhirClient fhirClient;

        public TestService(IFhirClient fhirClient)
        {
            this.fhirClient = fhirClient;
        }

        public ServerFhirResponse AddMeta(IKey key, Parameters parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse CapabilityStatement(string serverVersion)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse ConditionalCreate(IKey key, Resource resource, IEnumerable<Tuple<string, string>> parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse ConditionalCreate(IKey key, Resource resource, SearchParams parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse ConditionalDelete(IKey key, IEnumerable<Tuple<string, string>> parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse ConditionalUpdate(IKey key, Resource resource, SearchParams _params)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Create(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Delete(IKey key)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Delete(Entry entry)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Document(IKey key)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Everything(IKey key)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse GetPage(string snapshotkey, int index)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse History(HistoryParameters parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse History(string type, HistoryParameters parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse History(IKey key, HistoryParameters parameters)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Mailbox(Bundle bundle, Binary body)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Put(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Put(Entry entry)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Read(IKey key, ConditionalHeaderParameters parameters = null)
        {
            //ValidateKey(key);
            DomainResource patient = fhirClient.Read<DomainResource>(key.ToRelativeUri().ToString());

            FhirResponseFactory responseFactory = new FhirResponseFactory();
            Entry entry = Entry.Create(Bundle.HTTPVerb.GET, key, patient);

            return responseFactory.GetFhirResponse(entry, key, parameters);
        }

        public ServerFhirResponse ReadMeta(IKey key)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Search(string type, SearchParams searchCommand, int pageIndex = 0)
        {
            Bundle bundle = fhirClient.Search(searchCommand, type);

            FhirResponseFactory responseFactory = new FhirResponseFactory();
            return responseFactory.GetFhirResponse(bundle);
        }

        public ServerFhirResponse Transaction(IList<Entry> interactions)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Transaction(Bundle bundle)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse Update(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse ValidateOperation(IKey key, Resource resource)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse VersionRead(IKey key)
        {
            throw new NotImplementedException();
        }

        public ServerFhirResponse VersionSpecificUpdate(IKey versionedkey, Resource resource)
        {
            throw new NotImplementedException();
        }

        /*private FhirResponse CreateSnapshotResponse(Snapshot snapshot, int pageIndex = 0)
        {
            IPagingService pagingExtension = this.FindExtension<IPagingService>();
            IResourceStorageService resourceStorage = this.FindExtension<IResourceStorageService>();
            if (pagingExtension == null)
            {
                Bundle bundle = new Bundle()
                {
                    Type = snapshot.Type,
                    Total = snapshot.Count
                };
                bundle.Append(resourceStorage.Get(snapshot.Keys));
                return responseFactory.GetFhirResponse(bundle);
            }
            else
            {
                Bundle bundle = pagingExtension.StartPagination(snapshot).GetPage(pageIndex);
                return responseFactory.GetFhirResponse(bundle);
            }
        } */
    }
}