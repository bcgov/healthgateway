/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HealthGateway.Engine.Core
{
    public static class FhirMediaType
    {
        public const string OCTET_STREAM_TYPE = "application/octet-stream";

        public static readonly IList<String> StrictFormats = new ReadOnlyCollection<string>(
            new List<String> { ContentType.XML_CONTENT_HEADER, ContentType.JSON_CONTENT_HEADER });

        public static string[] LooseXmlFormats = { "xml", "text/xml", "application/xml" };

        public static readonly string[] LooseJsonFormats = { "json", "application/json" };

        /// <summary>
        /// Transforms loose formats to their strict variant
        /// </summary>
        /// <param name="format">Mime type</param>
        /// <returns></returns>
        public static string Interpret(string format)
        {
            if (format == null)
            {
                return ContentType.XML_CONTENT_HEADER;
            }
            else if (StrictFormats.Contains(format))
            {
                return format;
            }
            else if (LooseXmlFormats.Contains(format))
            {
                return ContentType.XML_CONTENT_HEADER;
            }
            else if (LooseJsonFormats.Contains(format))
            {
                return ContentType.JSON_CONTENT_HEADER;
            }
            else
            {
                return format;
            }
        }

        public static string GetContentType(Type type, ResourceFormat format)
        {
            if (typeof(Resource).IsAssignableFrom(type) || type == typeof(Resource))
            {
                switch (format)
                {
                    case ResourceFormat.Json:
                        return Hl7.Fhir.Rest.ContentType.JSON_CONTENT_HEADER;
                    case ResourceFormat.Xml:
                        return Hl7.Fhir.Rest.ContentType.XML_CONTENT_HEADER;
                    default:
                        return Hl7.Fhir.Rest.ContentType.XML_CONTENT_HEADER;
                }
            }
            else
            {
                // If type is a resource fallback to binary
                return OCTET_STREAM_TYPE;
            }
        }

        public static string GetMediaTypeFromRequest(HttpRequestMessage request)
        {
            MediaTypeHeaderValue headervalue = request.Content.Headers.ContentType;
            string s = (headervalue != null) ? headervalue.MediaType : null;
            return Interpret(s);
        }

        public static MediaTypeHeaderValue GetMediaTypeHeaderValue(Type type, ResourceFormat format)
        {
            string mediatype = FhirMediaType.GetContentType(type, format);
            MediaTypeHeaderValue header = new MediaTypeHeaderValue(mediatype);
            header.CharSet = Encoding.UTF8.WebName;
            return header;
        }
    }
}