using System.Collections.Generic;
using System.Linq;
using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class CreateVariablesCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        private readonly List<int> _variableSetSizes = new List<int>();
        private readonly int[] _defaultVariableSetSize = { 3, 10, 50, 100, 200 };

        public CreateVariablesCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void AddVariableSetSize(int size)
        {
            _variableSetSizes.Add(size);
        }

        private int[] GetVariableSetSize()
        {
            return _variableSetSizes.Any() ? _variableSetSizes.ToArray() : _defaultVariableSetSize;
        }

        public void Run()
        {
            var variablesMonkey = new VariablesMonkey(_repository);
            variablesMonkey.CreateVariables(GetVariableSetSize());
        }
    }
}