using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HealthGateway.Engine.Core
{
    public static class HttpHeaderUtil
    {
        public static bool Exists(IHeaderDictionary headers, string key)
        {
            return headers.GetCommaSeparatedValues(key).Count() > 0;
        }

        public static void Replace(IHeaderDictionary headers, string key, string value)
        {
            headers.Remove(key);
            headers.Add(key, value);
        }

        public static string Value(IHeaderDictionary headers, string key)
        {
            IEnumerable<string> values = headers.GetCommaSeparatedValues(key);
            return values.FirstOrDefault();
        }

        public static void ReplaceHeader(HttpRequest request, string header, string value)
        {
            HttpHeaderUtil.Replace(request.Headers, header, value);
        }

        public static string Header(HttpRequest request, string key)
        {
            return HttpHeaderUtil.Value(request.Headers, key);
        }

        public static string GetParameter(HttpRequest request, string key)
        {
            return FhirHttpUtil.GetStringParameter(request, key);
        }

        public static List<Tuple<string, string>> TupledParameters(HttpRequest request)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            using (IEnumerator<KeyValuePair<string, StringValues>> query = request.Query.GetEnumerator())
            {
                while (query.MoveNext())
                {
                    KeyValuePair<string, StringValues> pair = query.Current;
                    list.Add(new Tuple<string, string>(pair.Key, pair.Value));
                }
            }
            return list;
        }

        public static SearchParams GetSearchParams(HttpRequest request)
        {
            List<Tuple<string, string>> parameters = HttpHeaderUtil.TupledParameters(request).Where(tp => tp.Item1 != "_format").ToList();
            SearchParams searchCommand = SearchParams.FromUriParamList(parameters);
            return searchCommand;
        }
    }
}