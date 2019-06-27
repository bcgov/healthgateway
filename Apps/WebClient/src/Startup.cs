using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hl7.Fhir.Rest;

namespace WebClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IEnvironment), typeof(EnvironmentManager));

            // Not sure if this is what we want. Ideally this would be using the singleton
            EnvironmentManager env = new EnvironmentManager();

            // Test Service
            services.AddTransient<IPatientService>(serviceProvider =>
            {
                string url = env.GetValue("IMMUNIZATION_URL") + env.GetValue("IMMUNIZATION_VERSION") + env.GetValue("IMMUNIZATION_PATH");
                IFhirClient client = new FhirClient(url)
                {
                    Timeout = (60 * 1000),
                    PreferredFormat = ResourceFormat.Json
                };

                return new RestfulPatientService(client);
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
