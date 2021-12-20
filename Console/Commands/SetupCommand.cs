using Octopus.Client;
using SeaMonkey.Monkeys;
using SeaMonkey.ProbabilitySets;

namespace SeaMonkey.Commands
{
    public class SetupCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int MinStepsPerProject { get; set; } = 1;
        public int MaxStepsPerProject { get; set; } = 3;
        public int NumberOfProjectGroups { get; set; } = 10;

        public SetupCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var setupMonkey = new SetupMonkey(_repository)
            {
                StepsPerProject = new LinearProbability(MinStepsPerProject, MaxStepsPerProject)
            };
            setupMonkey.CreateProjectGroups(NumberOfProjectGroups);
        }
    }
}