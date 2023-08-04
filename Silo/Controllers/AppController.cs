// ------------------------------------------------------------------------------------------
// <copyright file="AppController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Silo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppController : Controller
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<AppController> _logger;
    private readonly ActivitySource _activitySource;
    private readonly Counter<int> _numOfRequests;

    public AppController(IGrainFactory grainFactory, ILogger<AppController> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
        _activitySource = new ActivitySource(typeof(AppController).Namespace ?? "Silo");
        Meter _meter = new("AppRequests");
        _numOfRequests = _meter.CreateCounter<int>("number_of_requests");
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        using var activity = _activitySource?.StartActivity($"{nameof(AppController)}.GetApp");
        try
        {
            _logger.LogInformation("Open Telemetry Logs");
            var grain = _grainFactory.GetGrain<IHelloGrain>("hi");

            activity?.SetBaggage("ParentBaggage", "This is how to pass details between parent and child");
            var greeting = await grain.SayHello("Yes");

            Activity.Current?.AddTag("ControllerName", "App");
            activity?.AddTag("Greeting", greeting.ToString());

            var metricDimension = new KeyValuePair<string, object?>("ControllerName", nameof(AppController));
            _numOfRequests.Add(1, metricDimension);

            return Ok(new { greeting });

        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}