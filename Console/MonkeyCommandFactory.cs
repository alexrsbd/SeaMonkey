using System;
using Octopus.Client;
using SeaMonkey.Commands;

namespace SeaMonkey
{
    public class MonkeyCommandFactory : IMonkeyCommandFactory
    {
        private readonly ITroopRequest _request;
        private readonly OctopusRepository _repository;

        public MonkeyCommandFactory(ITroopRequest request)
        {
            _request = request;

            var endpoint = new OctopusServerEndpoint(_request.ServerAddress, _request.ApiKey);
            _repository = new OctopusRepository(endpoint);
        }

        public IMonkeyCommand GetCommand(MonkeyBreed breed)
        {
            IMonkeyCommand command = breed switch
            {
                MonkeyBreed.CleanupVariables => new CleanupVariablesCommand(_repository),
                MonkeyBreed.Configuration => new ConfigurationCommand(_repository),
                MonkeyBreed.CreateVariables => new CreateVariablesCommand(_repository),
                MonkeyBreed.DeployForAllProjects => new DeployForAllProjectsCommand(_repository),
                MonkeyBreed.Infrastructure => new InfrastructureCommand(_repository),
                MonkeyBreed.Library => new LibraryCommand(_repository),
                MonkeyBreed.RunbookRun => new RunbookRunCommand(_repository),
                MonkeyBreed.Setup => new SetupCommand(_repository),
                MonkeyBreed.Tenant => new TenantCommand(_repository),
                _ => throw new ArgumentOutOfRangeException()
            };
            return command;
        }
    }
}