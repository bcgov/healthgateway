using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Hl7.Fhir.Rest;

namespace HealthGateway.Engine.Core
{
    class FhirHttpUtil
    {
        public static string GetStringParameter(HttpRequest request, string name)
        {
            string param = request.Query[name].ToString();
            if (param == null)
            {
                return null;
            }
            return param;
        }

        public static DateTimeOffset? GetDateParameter(HttpRequest request, string name)
        {
            string param = request.Query[name].ToString();
            if (param == null)
            {
                return null;
            }
            return DateTimeOffset.Parse(param);
        }

        public static int? GetIntParameter(HttpRequest request, string name)
        {
            string param = request.Query[name].ToString();
            return (int.TryParse(param, out int number)) ? number : (int?)null;
        }

        public static bool? GetBooleanParameter(HttpRequest request, string name)
        {
            string param = request.Query[name].ToString();
            return (bool.TryParse(param, out bool booleanParam)) ? booleanParam : (bool?)null;
        }

        public static DateTimeOffset? IfModifiedSince(HttpRequest request)
        {
            string param = request.Headers["If-Modified-Since"].ToString();
            DateTimeOffset date;
            if (DateTimeOffset.TryParse(param, out date))
            {
                return date;
            }
            {
                return null;
            }
        }

        public static IEnumerable<string> IfNoneMatch(HttpRequest request)
        {
            // The if-none-match can be either '*' or tags. This needs further implementation.
            return request.Headers.GetCommaSeparatedValues("If-Modified-Since");
        }

        private static string WithoutQuotes(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            else
            {
                return s.Trim('"');
            }
        }

        public static string GetValue(HttpRequest request, string key)
        {
            if (request.Headers.Count() > 0)
            {
                string param = request.Headers[key].ToString();
                return param;
            }
            else
            {
                return null;
            }
        }

        public static bool PreferRepresentation(HttpRequest request)
        {
            string value = FhirHttpUtil.GetValue(request, "Prefer");
            return (value == "return=representation" || value == null);
        }

        public static string IfMatchVersionId(HttpRequest request)
        {
            if (request.Headers.Count() > 0)
            {
                List<string> parameters = request.Headers.GetCommaSeparatedValues("If-Match").ToList();

                var tag = parameters.FirstOrDefault();
                if (tag != null)
                {
                    return WithoutQuotes(tag);
                }
                else
                {
                    return null;
                }
                // todo: validate missing quotes
                //else 
                //{
                //    does it need quotes?
                //}
            }
            else
            {
                return null;
            }
        }

        public static SummaryType RequestSummary(HttpRequest request)
        {
            string summaryString = request.Query["_summary"];
            if (string.IsNullOrWhiteSpace(summaryString))
            {
                return SummaryType.False;
            }
            else
            {
                SummaryType? summaryType = null;
                switch (summaryString)
                {
                    case "true":
                        summaryType = SummaryType.True;
                        break;
                    case "text":
                        summaryType = SummaryType.Text;
                        break;
                    case "data":
                        summaryType = SummaryType.Data;
                        break;
                    case "count":
                        summaryType = SummaryType.Count;
                        break;
                    default:
                        summaryType = null;
                        break;
                }

                return summaryType.HasValue ? summaryType.Value : SummaryType.False;
            }
        }
    }
}