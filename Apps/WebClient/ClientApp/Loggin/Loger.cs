using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebClient.Log
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"Send.txt", true))
            {
                file.WriteLine("Fourth line");

                file.WriteLine("Request:");
                file.WriteLine(request.ToString());
                if (request.Content != null)
                {
                    file.WriteLine(await request.Content.ReadAsStringAsync());
                }
                file.WriteLine();

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                file.WriteLine("Response:");
                file.WriteLine(response.ToString());
                if (response.Content != null)
                {
                    file.WriteLine(await response.Content.ReadAsStringAsync());
                }
                file.WriteLine();

                file.Flush();

                return response;
            }
        }
    }
}