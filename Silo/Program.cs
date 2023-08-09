// ------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using Azure.Monitor.OpenTelemetry.Exporter;
using Grains;
using OpenTelemetry.Exporter.Geneva;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
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
        var genevaMetricsConnectionString = appTelemetryConn.GenevaMetricsConnectionString;

        services.AddControllers();
        services.AddOpenTelemetry()
        .WithTracing(builder =>
        {
            builder
                // Add sources of traces
                .AddSource("*") // adds all traces
                .AddSource("Azure.*") // Adds traces for azure third-party calls
                .AddSource("OpenTelemtry.*")
                .AddSource(
                    typeof(AppController).Namespace,
                    typeof(HelloGrain).Namespace,
                    typeof(Program).Assembly.GetName().Name)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "OpenTelemetry", serviceVersion: "1.0"))
                .SetErrorStatusOnException()
                // !! important to get .NET traces
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

                    opts.EnrichWithHttpRequest = (activity, incomingRequest) =>
                    {
                        var correlationId = incomingRequest.Headers["x-ms-correlation-request-id"].Any()
                                    ? string.Join(", ", incomingRequest.Headers["x-ms-correlation-request-id"])
                                    : Guid.NewGuid().ToString();

                        activity.AddBaggage("correlationId", correlationId);
                    };

                    opts.EnrichWithException = (activity, ex) =>
                    {
                        activity.SetStatus(ActivityStatusCode.Error);
                    };
                })
                .AddConsoleExporter(); // Exports Traces to Console

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
        })
        .WithMetrics(builder =>
        {
            builder
                .AddAspNetCoreInstrumentation() // Adds basic .NET metrics
                .AddRuntimeInstrumentation() // Adds basic runtime metrics
                .AddMeter("AppRequests") // Add name of the meter you created so it has the source
                .AddConsoleExporter(); // Exports Metrics to console

            // Export Metrics to Application Insights
            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                builder.AddAzureMonitorMetricExporter(opts =>
                {
                    opts.ConnectionString = appInsightsConnectionString;
                });

            }
            // Export Metrics to Geneva
            if (!string.IsNullOrWhiteSpace(genevaMetricsConnectionString))
            {
                builder.AddGenevaMetricExporter(opts =>
                {
                    opts.ConnectionString = genevaMetricsConnectionString;

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
             options.AddProcessor(new LogProcessor());
             options.IncludeFormattedMessage = true;
             options.IncludeScopes = true;
             options.ParseStateValues = true;
         });
     })
    .RunConsoleAsync();