using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class DeployForAllProjectsCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int MaxNumberOfDeployments { get; set; } = 100;

        public DeployForAllProjectsCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var deployMonkey = new DeployMonkey(_repository);
            deployMonkey.RunForAllProjects(maxNumberOfDeployments: MaxNumberOfDeployments);
        }
    }
}