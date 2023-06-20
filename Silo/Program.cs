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
    .ConfigureServices(services =>
    {
        services.AddControllers();
    })
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())

    })
    .ConfigureLogging(logging => logging.AddConsole())
    .RunConsoleAsync();