using System.Net;
using System;
using Hl7.Fhir.Rest;

namespace WebClient.Log
{
    public class LogFhirClient : FhirClient
    {

        public LogFhirClient(Uri endpoint, bool verifyFhirVersion = false) : base(endpoint, verifyFhirVersion) { }
        public LogFhirClient(string endpoint, bool verifyFhirVersion = false) : base(endpoint, verifyFhirVersion) { }

        public LogFhirClient(Uri endpoint) : base(endpoint) { }

        protected override void BeforeRequest(HttpWebRequest rawRequest, byte[] body)
        {
            //Do not log now
            //logRequest(rawRequest, body);
            base.BeforeRequest(rawRequest, body);
        }

        protected override void AfterResponse(HttpWebResponse webResponse, byte[] body)
        {
            //Do not log now
            //logResponse(webResponse, body);
            base.AfterResponse(webResponse, body);
        }

        private void logRequest(HttpWebRequest rawRequest, byte[] body)
        {
            string now = "[" + DateTime.Now.ToString() + "] ";
            string bodyString = "";
            if (body != null && body.Length != 0)
            {
                bodyString = System.Text.Encoding.UTF8.GetString(body);
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./Send.txt", true))
            {
                file.WriteLine();
                file.WriteLine(now + "Request:");
                if (rawRequest.Headers.Count > 0)
                {
                    file.WriteLine(now + "Headers:");
                    file.WriteLine(rawRequest.Headers.ToString());
                }

                file.WriteLine(now + "Body:");
                file.WriteLine(bodyString);

                file.WriteLine(now + rawRequest.ToString());
                file.WriteLine();
                file.Flush();
            }
        }

        private void logResponse(HttpWebResponse webResponse, byte[] body)
        {
            string now = "[" + DateTime.Now.ToString() + "] ";
            string bodyString = "";
            if (body != null && body.Length != 0)
            {
                bodyString = System.Text.Encoding.UTF8.GetString(body);
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./Send.txt", true))
            {
                file.WriteLine();
                file.WriteLine(now + "Response:");
                if (webResponse.Headers.Count > 0)
                {
                    file.WriteLine(now + "Headers:");
                    file.WriteLine(webResponse.Headers.ToString());
                }

                file.WriteLine(now + "Body:");
                file.WriteLine(bodyString);

                file.WriteLine(now + webResponse.ToString());
                file.WriteLine();
                file.Flush();
            }
        }
    }
}