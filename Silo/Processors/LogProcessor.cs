// ------------------------------------------------------------------------------------------
// <copyright file="LogProcessor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class LogProcessor : BaseProcessor<LogRecord>
{
    private readonly string name;

    public LogProcessor(string name = "LogProcessor")
    {
        this.name = name;
    }

    public override void OnEnd(LogRecord data)
    {
        var logState = new List<KeyValuePair<string, object?>>
        {
            new("correlationId", Activity.Current?.GetBaggageItem("correlationId")),
            new("version", Activity.Current ?.GetBaggageItem("version")),

        };

        if (data.Attributes != null)
        {
            var state = data.Attributes.ToList();
            data.Attributes = new ReadOnlyCollectionBuilder<KeyValuePair<string, object?>>(state.Concat(logState))
                .ToReadOnlyCollection();
        }

        base.OnEnd(data);
    }

    protected override void Dispose(bool disposing)
    {
        Console.WriteLine($"{this.name}.Dispose({disposing})");
    }
}