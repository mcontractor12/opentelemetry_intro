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

    public AppController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var grain = _grainFactory.GetGrain<IHelloGrain>("hi");
        var greeting = await grain.SayHello("Yes");
        return Ok(new { greeting });
    }
}