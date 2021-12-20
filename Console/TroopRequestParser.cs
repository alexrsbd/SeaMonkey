using System.Collections.Generic;
using Octopus.CoreUtilities.Extensions;
using Octopus.Shared.Internals.Options;
using SeaMonkey.Exceptions;

namespace SeaMonkey
{
    public class TroopRequestParser : ITroopRequestParser
    {
        public TroopRequest Parse(string[] args)
        {
            if (args.IsNullOrEmpty())
                throw new InvalidTroopRequestException(
                    "No arguments specified. Please provide a server, apiKey and some monkeys you wish to run. E.g. SeaMonkey.exe --server=http://localhost:8065 --apiKey=API-1234 --runConfigurationMonkey --runLibraryMonkey");

            var data = ParseOptions(args);

            var request = new TroopRequest(data.ServerAddress, data.ApiKey, data.MonkeyBreeds);
            return request;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private ParsedArgs ParseOptions(string[] args)
        {
            var parsedArgs = new ParsedArgs();

            var options = new OptionSet();

            options.Add<string>("server=", "The Octopus Server URI, E.g. http://localhost:8065",
                e => parsedArgs.ServerAddress = e);

            options.Add<string>("apiKey=", "The Octopus API key, E.g. API-1234",
                e => parsedArgs.ApiKey = e);

            options.Add<string>("runSetupMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.Setup));

            options.Add<string>("runTenantMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.Tenant));

            options.Add<string>("runDeployMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.DeployForAllProjects));

            options.Add<string>("runRunbookRunMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.RunbookRun));

            options.Add<string>("runConfigurationMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.Configuration));

            options.Add<string>("runInfrastructureMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.Infrastructure));

            options.Add<string>("runLibraryMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.Library));

            options.Add<string>("runVariablesMonkey", "",
                e => parsedArgs.MonkeyBreeds.Add(MonkeyBreed.CreateVariables));

            options.Parse(args);

            return parsedArgs;
        }

        private class ParsedArgs
        {
            public string ServerAddress { get; set; }
            public string ApiKey { get; set; }
            public SortedSet<MonkeyBreed> MonkeyBreeds { get; } = new SortedSet<MonkeyBreed>();
        }
    }
}