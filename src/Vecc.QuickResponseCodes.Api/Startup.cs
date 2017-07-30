using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using Vecc.QuickResponseCodes.Api.SwaggerExamples;

namespace Vecc.QuickResponseCodes.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddQuickResponseCodes();

            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new Info
                                                          {
                                                              Title = "Api - V1",
                                                              Version = "v1"
                                                          });
                                       c.DescribeAllEnumsAsStrings();
                                       c.DescribeStringEnumsInCamelCase();
                                       c.SchemaFilter<AddV1GetQrCodeResponseExample>();

                                       var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "api.xml");
                                       c.IncludeXmlComments(filePath);
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api - v1"); });

            app.UseMvc();
        }
    }
}
