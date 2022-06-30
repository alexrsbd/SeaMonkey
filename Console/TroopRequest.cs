using System.Collections.Generic;
using Octopus.CoreUtilities.Extensions;
using SeaMonkey.Exceptions;

namespace SeaMonkey
{
    public class TroopRequest : ITroopRequest
    {
        private readonly SortedSet<MonkeyBreed> _monkeyBreeds;

        public string ServerAddress { get; }
        public string ApiKey { get; }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        public TroopRequest(string serverAddress, string apiKey, ICollection<MonkeyBreed> monkeyBreeds)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new InvalidTroopRequestException(
                    "No server specified. Please use the --server parameter to specify an Octopus server instance.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidTroopRequestException(
                    "No apiKey specified. Please use the --apiKey parameter to specify an Octopus API key.");

            if (monkeyBreeds.IsNullOrEmpty())
                throw new InvalidTroopRequestException(
                    "No monkeys were specified. Please use one of the following flags to run a monkey: --runSetupMonkey --runTenantMonkey --runDeployMonkey --runConfigurationMonkey --runInfrastructureMonkey --runLibraryMonkey --runVariablesMonkey --runSpacesMonkey");

            ServerAddress = serverAddress;
            ApiKey = apiKey;
            _monkeyBreeds = new SortedSet<MonkeyBreed>(monkeyBreeds);
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Global
        public IReadOnlyCollection<MonkeyBreed> GetMonkeyBreeds()
        {
            return _monkeyBreeds;
        }
    }
}