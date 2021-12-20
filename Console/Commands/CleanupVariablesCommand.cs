using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class CleanupVariablesCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;

        public CleanupVariablesCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var variablesMonkey = new VariablesMonkey(_repository);
            variablesMonkey.CleanupVariables();
        }
    }
}