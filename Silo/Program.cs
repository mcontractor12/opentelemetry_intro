using Azure.Monitor.OpenTelemetry.Exporter;
using Grains;
using OpenTelemetry.Exporter.Geneva;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans;
using Orleans.Hosting;
using Silo.Configuration;
using Silo.Controllers;
using System.Diagnostics;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure((ctx, app) =>
        {
            if (ctx.HostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });
    })
    .ConfigureServices((ctx, services) =>
    {
        var appTelemetryConn = ctx.Configuration.GetSection(nameof(AppExporters)).Get<AppExporters>();
        var genevaConnString = appTelemetryConn.GenevaConnectionString;
        var appInsightsConnectionString = appTelemetryConn.AppInsightsConnectionString;

        services.AddControllers();
        services.AddOpenTelemetry()
        .WithTracing(builder =>
        {
            builder
                // Adds traces for azure third-party calls
                .AddSource("*")
                .AddSource("Azure.*")
                .AddSource("OpenTelemtry.*")
                .AddSource(
                    typeof(AppController).Namespace,
                    typeof(HelloGrain).Namespace,
                    typeof(Program).Assembly.GetName().Name)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "OpenTelemetry", serviceVersion: "1.0"))
                .SetErrorStatusOnException()
                .AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.EnrichWithHttpResponse = (activity, response) =>
                    {
                        if (response?.StatusCode >= StatusCodes.Status400BadRequest)
                        {
                            activity.SetStatus(ActivityStatusCode.Error);
                        }
                        activity.SetTag("response.Length", response?.ContentLength);
                    };
                    opts.EnrichWithException = (activity, ex) =>
                    {
                        activity.SetStatus(ActivityStatusCode.Error);
                    };
                })

                // Exports Traces to Console
                .AddConsoleExporter();

            // Export Traces to Application Insights
            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                builder.AddAzureMonitorTraceExporter(opts =>
                {
                    opts.ConnectionString = appInsightsConnectionString;
                });

            }

            // Export Traces to Geneva
            if (!string.IsNullOrWhiteSpace(genevaConnString))
            {
                builder.AddGenevaTraceExporter(opts =>
                {
                    opts.ConnectionString = genevaConnString;

                });

            }
        });
    })
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .ConfigureLogging(logging => logging.AddConsole());

    })
     .ConfigureLogging((ctx, loggingBuilder) =>
     {
         var appTelemetryConn = ctx.Configuration.GetSection(nameof(AppExporters)).Get<AppExporters>();
         var genevaConnString = appTelemetryConn.GenevaConnectionString;
         var appInsightsConnectionString = appTelemetryConn.AppInsightsConnectionString;

         loggingBuilder.ClearProviders().AddOpenTelemetry(options =>
         {
             // Export logs to Geneva
             if (!string.IsNullOrWhiteSpace(genevaConnString))
             {
                 options.AddGenevaLogExporter(genevaConn => genevaConn.ConnectionString = genevaConnString);
             }

             // Export logs to Application Insights
             if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
             {
                 options.AddAzureMonitorLogExporter(appConn =>
                 {
                     appConn.ConnectionString = appInsightsConnectionString;
                 });
             }


             //Export Logs to Console
             options.AddConsoleExporter();
             options.IncludeFormattedMessage = true;
             options.IncludeScopes = true;
             options.ParseStateValues = true;
         });
     })
    .RunConsoleAsync();