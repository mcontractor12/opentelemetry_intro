// ------------------------------------------------------------------------------------------
// <copyright file="HelloGrain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Grains;

public class HelloGrain : Grain, IHelloGrain
{
    private readonly ILogger<HelloGrain> _logger;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _logger = logger;
    }

    public Task<string> SayHello(string greeting)
    {
        _logger.LogDebug($"Say Hello message received: greeting = '{greeting}'");
        return Task.FromResult($"You say: '{greeting}', I say Hello!");
    }
}