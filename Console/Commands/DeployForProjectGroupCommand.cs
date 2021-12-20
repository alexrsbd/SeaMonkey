using System;
using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class DeployForProjectGroupCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public string ProjectGroupName { get; set; }
        public TimeSpan DelayBetween { get; set; } = TimeSpan.FromSeconds(5);

        public DeployForProjectGroupCommand(OctopusRepository repository, string projectGroupName)
        {
            _repository = repository;
            ProjectGroupName = projectGroupName;
        }

        public void Run()
        {
            var deployMonkey = new DeployMonkey(_repository);
            deployMonkey.RunForGroup(ProjectGroupName, DelayBetween);
        }
    }
}