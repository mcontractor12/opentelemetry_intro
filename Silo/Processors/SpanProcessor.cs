// ------------------------------------------------------------------------------------------
// <copyright file="LogProcessor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using OpenTelemetry;
using System.Diagnostics;

internal class SpanProcessor : BaseProcessor<Activity>
{
    private readonly string name;

    public SpanProcessor(string name = "SpanProcessor")
    {
        this.name = name;
    }

    public override void OnEnd(Activity data)
    {
        data.SetTag("correlationId", Activity.Current?.GetBaggageItem("correlationId"));
        data.SetTag("version", Activity.Current?.GetBaggageItem("version"));
    }

    protected override void Dispose(bool disposing)
    {
        Console.WriteLine($"{this.name}.Dispose({disposing})");
    }
}