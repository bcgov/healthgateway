namespace HealthGateway.WebClient
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    public static class Program
    {
        private const string EnvironmentPrefix = "HealthGateway_";

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables(prefix: EnvironmentPrefix);
                })
                .UseStartup<Startup>()
                .ConfigureKestrel(options =>
                {
                   options.Limits.MaxResponseBufferSize = options.Limits.MaxResponseBufferSize*20;
                   options.Limits.MaxRequestHeadersTotalSize = options.Limits.MaxRequestHeadersTotalSize*20;
                   options.Limits.MaxRequestBufferSize = options.Limits.MaxRequestBufferSize*20;
                   options.ConfigureEndpointDefaults(listenOptions =>
                    {
                        listenOptions.UseConnectionLogging();
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();
    }
}
