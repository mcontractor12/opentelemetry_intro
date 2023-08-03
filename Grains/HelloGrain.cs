// ------------------------------------------------------------------------------------------
// <copyright file="HelloGrain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using System.Diagnostics;

namespace Grains;

public class HelloGrain : Grain, IHelloGrain
{
    private readonly ILogger<HelloGrain> _logger;
    private readonly ActivitySource _activitySource;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource(typeof(HelloGrain).Namespace ?? "Silo.Grain");
    }

    public Task<string> SayHello(string greeting)
    {
        using var activity = _activitySource?.StartActivity($"{nameof(HelloGrain)}.{nameof(SayHello)}");
        _logger.LogInformation($"Say Hello message received: greeting = '{greeting}'");

        //activity?.AddBaggage("baggage", "This is how to pass details between parent and child");
        var baggage = Activity.Current?.GetBaggageItem("ParentBaggage");
        activity?.AddTag("ParentBaggage", baggage);

        return Task.FromResult($"You say: '{greeting}', I say Hello!");
    }
}