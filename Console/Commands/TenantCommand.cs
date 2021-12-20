using Octopus.Client;
using SeaMonkey.Monkeys;

namespace SeaMonkey.Commands
{
    public class TenantCommand : IMonkeyCommand
    {
        private readonly OctopusRepository _repository;
        public int NumberOfTenants { get; set; } = 50;

        public TenantCommand(OctopusRepository repository)
        {
            _repository = repository;
        }

        public void Run()
        {
            var tenantMonkey = new TenantMonkey(_repository);
            tenantMonkey.Create(NumberOfTenants);
        }
    }
}