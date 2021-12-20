using System.Collections.Generic;

namespace SeaMonkey
{
    public interface ITroopRequest
    {
        string ServerAddress { get; }
        string ApiKey { get; }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        IReadOnlyCollection<MonkeyBreed> GetMonkeyBreeds();
    }
}