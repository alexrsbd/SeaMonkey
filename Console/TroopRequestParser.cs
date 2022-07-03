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

            var request = ParseOptions(args);
            return request;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private TroopRequest ParseOptions(string[] args)
        {
            var requestBuilder = new TroopRequestBuilder();

            var options = new OptionSet();

            options.Add<string>("server=", "The Octopus Server URI, E.g. http://localhost:8065",
                e => requestBuilder.WithServerAddress(e));

            options.Add<string>("apiKey=", "The Octopus API key, E.g. API-1234",
                e => requestBuilder.WithApiKey(e));

            options.Add<string>("runSetupMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Setup));

            options.Add<string>("runTenantMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Tenant));

            options.Add<string>("runDeployMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.DeployForAllProjects));

            options.Add<string>("runRunbookRunMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.RunbookRun));

            options.Add<string>("runConfigurationMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Configuration));

            options.Add<string>("runInfrastructureMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Infrastructure));

            options.Add<string>("runLibraryMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Library));

            options.Add<string>("runVariablesMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.CreateVariables));

            options.Add<string>("runSpacesMonkey", "",
                e => requestBuilder.WithMonkeyBreed(MonkeyBreed.Spaces));

            options.Parse(args);

            return requestBuilder.Build();
        }
    }
}