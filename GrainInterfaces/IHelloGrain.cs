// ------------------------------------------------------------------------------------------
// <copyright file="IHelloGrain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved
// </copyright>
// ------------------------------------------------------------------------------------------

using Orleans;

namespace GrainInterfaces;

public interface IHelloGrain : IGrainWithStringKey
{
    Task<string> SayHello(string greeting);
}