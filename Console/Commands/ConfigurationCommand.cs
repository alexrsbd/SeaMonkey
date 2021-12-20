using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class ConfigurationCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int NumberOfSubscription { get; set; } = 70;
        public int NumberOfTeams { get; set; } = 70;
        public int NumberOfUsers { get; set; } = 70;
        public int NumberOfUserRoles { get; set; } = 70;

        public ConfigurationCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var configurationMonkey = new ConfigurationMonkey(_repository);
            configurationMonkey.CreateRecords(NumberOfSubscription, NumberOfTeams, NumberOfUsers, NumberOfUserRoles);
        }
    }
}