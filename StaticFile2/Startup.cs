using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StaticFile2
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/internals/health", new HealthCheckOptions {ResponseWriter = WriteResponse});
            });
        }

        /// <summary>
        /// Configuration for serialization of the health status.
        /// </summary>
        private static readonly JsonSerializerOptions HealthSerializerOptions = new()
        {
            Converters =
            {
                // the status is an enum, which is serialized as its numeric value. (IE: Healthy = 2).
                // This converts the enum (2) to it's string value ("Healthy").
                new JsonStringEnumConverter(),
            }
        };

        /// <summary>
        /// Returns the health status as a json document.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        private static Task WriteResponse(HttpContext context, HealthReport report)
        {
            // in this simple service, the only thing of interest in the report is the status property.
            // return it as json.
            // by default, it would've returned the status as text, not json.
            // feel free to tweak. Lacking a spec, Status: Healthy is good enough for now,
            // but you can add anything that you like.
            var json = JsonSerializer.Serialize(new {report.Status}, HealthSerializerOptions);
            return context.Response.WriteAsync(json);
        }
    }
}