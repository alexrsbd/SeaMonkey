using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class RunbookRunCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int MaxNumberOfRunbookRuns { get; set; } = 100;

        public RunbookRunCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var runbookRunMonkey = new RunbookRunMonkey(_repository);
            runbookRunMonkey.RunForAllRunbooks(maxNumberOfRunbookRuns: MaxNumberOfRunbookRuns);
        }
    }
}