using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class InfrastructureCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;

        public int NumberOfMachinePolicies { get; set; } = 7;
        public int NumberOfProxies { get; set; } = 7;
        public int NumberOfUserAccounts { get; set; } = 7;
        public int NumberOfMachines { get; set; } = 70;
        public int NumberOfWorkerPools { get; set; } = 2;
        public int NumberOfWorkersPerPool { get; set; } = 2;

        public InfrastructureCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var infrastructureMonkey = new InfrastructureMonkey(_repository);
            infrastructureMonkey.CreateRecords(NumberOfMachinePolicies, NumberOfProxies, NumberOfUserAccounts,
                NumberOfMachines, NumberOfWorkerPools, NumberOfWorkersPerPool);
        }
    }
}