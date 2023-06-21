// ------------------------------------------------------------------------------------------
// <copyright file="AppController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using GrainInterfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Silo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppController : Controller
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<AppController> _logger;

    public AppController(IGrainFactory grainFactory, ILogger<AppController> logger)
    {
        _grainFactory = grainFactory;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Open Telemetry Logs");
        var grain = _grainFactory.GetGrain<IHelloGrain>("hi");
        var greeting = await grain.SayHello("Yes");
        return Ok(new { greeting });
    }
}