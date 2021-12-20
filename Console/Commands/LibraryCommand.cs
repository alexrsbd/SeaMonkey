using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class LibraryCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int NumberOfFeeds { get; set; } = 70;
        public int NumberOfScriptModules { get; set; } = 70;
        public int NumberOfLibraryVariableSets { get; set; } = 10;
        public int NumberOfLibraryVariableVariables { get; set; } = 3;
        public int NumberOfTagSets { get; set; } = 70;
        public int NumberOfCertificates { get; set; } = 50;

        public LibraryCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var libraryMonkey = new LibraryMonkey(_repository);
            libraryMonkey.CreateRecords(NumberOfFeeds, NumberOfScriptModules, NumberOfLibraryVariableSets,
                NumberOfLibraryVariableVariables, NumberOfTagSets, NumberOfCertificates);
        }
    }
}