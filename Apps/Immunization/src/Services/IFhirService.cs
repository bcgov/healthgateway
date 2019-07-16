using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;

namespace HealthGateway.Service
{
    // Service interface inspired/based on https://github.com/FirelyTeam/spark
    public interface IFhirService
    {
        ServerFhirResponse AddMeta(IKey key, Parameters parameters);
        ServerFhirResponse ConditionalCreate(IKey key, Resource resource, IEnumerable<Tuple<string, string>> parameters);
        ServerFhirResponse ConditionalCreate(IKey key, Resource resource, SearchParams parameters);
        ServerFhirResponse ConditionalDelete(IKey key, IEnumerable<Tuple<string, string>> parameters);
        ServerFhirResponse ConditionalUpdate(IKey key, Resource resource, SearchParams _params);
        ServerFhirResponse CapabilityStatement(string serverVersion);
        ServerFhirResponse Create(IKey key, Resource resource);
        ServerFhirResponse Delete(IKey key);
        ServerFhirResponse Delete(Entry entry);
        ServerFhirResponse GetPage(string snapshotkey, int index);
        ServerFhirResponse History(HistoryParameters parameters);
        ServerFhirResponse History(string type, HistoryParameters parameters);
        ServerFhirResponse History(IKey key, HistoryParameters parameters);
        ServerFhirResponse Mailbox(Bundle bundle, Binary body);
        ServerFhirResponse Put(IKey key, Resource resource);
        ServerFhirResponse Put(Entry entry);
        ServerFhirResponse Read(IKey key, ConditionalHeaderParameters parameters = null);
        ServerFhirResponse ReadMeta(IKey key);
        ServerFhirResponse Search(string type, SearchParams searchCommand, int pageIndex = 0);
        ServerFhirResponse Transaction(IList<Entry> interactions);
        ServerFhirResponse Transaction(Bundle bundle);
        ServerFhirResponse Update(IKey key, Resource resource);
        ServerFhirResponse ValidateOperation(IKey key, Resource resource);
        ServerFhirResponse VersionRead(IKey key);
        ServerFhirResponse VersionSpecificUpdate(IKey versionedkey, Resource resource);
        ServerFhirResponse Everything(IKey key);
        ServerFhirResponse Document(IKey key);
    }
}