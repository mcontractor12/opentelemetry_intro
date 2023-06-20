using Orleans;

namespace GrainInterfaces;

public interface IHelloGrain : IGrainWithStringKey
{
    Task<string> SayHello(string greeting);
}