using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using HealthGateway.Engine.Core;

namespace HealthGateway.Engine.Extensions
{
    public static class EntryExtensions
    {
        public static Bundle.EntryComponent TranslateToSparseEntry(this Entry entry, ServerFhirResponse response = null)
        {
            var bundleEntry = new Bundle.EntryComponent();
            if (response != null)
            {
                bundleEntry.Response = new Bundle.ResponseComponent()
                {
                    Status = string.Format("{0} {1}", (int)response.StatusCode, response.StatusCode),
                    Location = response.Key != null ? response.Key.ToString() : null,
                    Etag = response.Key != null ? ETag.Create(response.Key.VersionId).ToString() : null,
                    LastModified =
                        (entry != null && entry.Resource != null && entry.Resource.Meta != null)
                            ? entry.Resource.Meta.LastUpdated
                            : null
                };
            }

            SetBundleEntryResource(entry, bundleEntry);
            return bundleEntry;
        }

        public static Bundle.EntryComponent ToTransactionEntry(this Entry entry)
        {
            var bundleEntry = new Bundle.EntryComponent();

            if (bundleEntry.Request == null)
            {
                bundleEntry.Request = new Bundle.RequestComponent();
            }
            bundleEntry.Request.Method = entry.Method;
            bundleEntry.Request.Url = entry.Key.ToUri().ToString();

            SetBundleEntryResource(entry, bundleEntry);

            return bundleEntry;
        }

        private static void SetBundleEntryResource(Entry entry, Bundle.EntryComponent bundleEntry)
        {
            if (entry.HasResource())
            {
                bundleEntry.Resource = entry.Resource;
                entry.Key.ApplyTo(bundleEntry.Resource);
                bundleEntry.FullUrl = entry.Key.ToUriString();
            }
        }

        public static bool HasResource(this Entry entry)
        {
            return (entry.Resource != null);
        }

        public static bool IsDeleted(this Entry entry)
        {
            // API: HTTPVerb should have a broader scope than Bundle.
            return entry.Method == Bundle.HTTPVerb.DELETE;
        }

        public static bool Present(this Entry entry)
        {
            return (entry.Method == Bundle.HTTPVerb.POST) || (entry.Method == Bundle.HTTPVerb.PUT);
        }


        public static void Append(this IList<Entry> list, IList<Entry> appendage)
        {
            foreach (Entry entry in appendage)
            {
                list.Add(entry);
            }
        }

        public static bool Contains(this IList<Entry> list, Entry item)
        {
            IKey key = item.Key;
            return list.FirstOrDefault(i => i.Key.EqualTo(item.Key)) != null;
        }

        public static void AppendDistinct(this IList<Entry> list, IList<Entry> appendage)
        {
            foreach (Entry item in appendage)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }

        public static IEnumerable<Resource> GetResources(this IEnumerable<Entry> entries)
        {
            return entries.Where(i => i.HasResource()).Select(i => i.Resource);
        }

        private static bool isValidResourcePath(string path, Resource resource)
        {
            string name = path.Split('.').FirstOrDefault();
            return resource.TypeName == name;
        }

        public static Bundle Replace(this Bundle bundle, IEnumerable<Entry> entries)
        {
            bundle.Entry = entries.Select(e => e.TranslateToSparseEntry()).ToList();
            return bundle;
        }

        // If an interaction has no base, you should be able to supplement it (from the containing bundle for example)
        public static void SupplementBase(this Entry entry, string _base)
        {
            Key key = entry.Key.Clone();
            if (!key.HasBase())
            {
                key.Base = _base;
                entry.Key = key;
            }
        }

        public static void SupplementBase(this Entry entry, Uri _base)
        {
            SupplementBase(entry, _base.ToString());
        }

        public static IEnumerable<Entry> Transferable(this IEnumerable<Entry> entries)
        {
            return entries.Where(i => i.State == EntryState.Undefined);
        }

        public static void Assert(this EntryState state, EntryState correct)
        {
            if (state != correct)
            {
                throw Error.Internal("Interaction was in an invalid state");
            }
        }
    }
}
