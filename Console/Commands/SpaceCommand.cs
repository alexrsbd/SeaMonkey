using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class SpaceCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int NumberOfSpaces { get; set; } = 5;

        public SpaceCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var spaceMonkey = new SpaceMonkey(_repository);
            spaceMonkey.Create(NumberOfSpaces);
        }
    }
}