using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Model.Accounts;
using Octopus.Client.Model.Endpoints;
using SeaMonkey.ProbabilitySets;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

namespace SeaMonkey.Monkeys
{
    public class InfrastructureMonkey : Monkey
    {
        private IntProbability RolesPerMachine { get; set; } = new LinearProbability(0, 4);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();
        private string[] PossibleRoles = new string[] {
            "Rick",
            "Morty",
            "Mr. Meeseeks",
            "Roy's Carpet Store",
        };

        public InfrastructureMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void CreateRecords(int numberOfMachinePolicies,
            int numberOfProxies,
            int numberOfUsernamePasswords,
            int numberOfMachines)
        {
            CreateEnvironments(0);
            CreateMachinePolicies(numberOfMachinePolicies);
            CreateProxies(numberOfProxies);
            CreateUsernamePasswordAccounts(numberOfUsernamePasswords);
            CreateMachines(numberOfMachines);
        }

        #region MachinePolicies

        public void CreateMachinePolicies(int numberOfRecords)
        {
            var currentCount = Repository.MachinePolicies.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateMachinePolicy(x);
        }

        private MachinePolicyResource CreateMachinePolicy(int prefix)
        {
            return
                Repository.MachinePolicies.Create(new MachinePolicyResource()
                {
                    Name = "MachinePolicy-" + prefix.ToString("000"),
                    Description = "MachinePolicy-" + prefix.ToString("000"),
                    IsDefault = false,
                    MachineHealthCheckPolicy = new MachineHealthCheckPolicy
                    {

                    },
                    MachineConnectivityPolicy = new MachineConnectivityPolicy
                    {

                    },
                    MachineCleanupPolicy = new MachineCleanupPolicy
                    {

                    },
                    MachineUpdatePolicy = new MachineUpdatePolicy
                    {

                    }
                });
    }

        #endregion

        #region Proxies

        public void CreateProxies(int numberOfRecords)
        {
            var currentCount = Repository.Proxies.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateProxy(x);
        }

        private ProxyResource CreateProxy(int prefix)
        {
            return
                Repository.Proxies.Create(new ProxyResource()
                {
                    Name = "Proxy-" + prefix.ToString("000"),
                    Host = "localhost",
                    Port = 80,
                });
        }

        #endregion

        #region Accounts

        public void CreateUsernamePasswordAccounts(int numberOfRecords)
        {
            var currentCount = Repository.Accounts.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateUsernamePasswordAccount(x);
        }

        private AccountResource CreateUsernamePasswordAccount(int prefix)
        {
            var password = new SensitiveValue();
            password.NewValue = "Asdf1234";
            var account = new UsernamePasswordAccountResource()
            {
                Name = "UsernamePasswordAccount-" + prefix.ToString("000"),
                Username = "User" + prefix.ToString("000"),
                Password = password,
            } as AccountResource;
            return Repository.Accounts.Create(account);
        }

        #endregion

        #region Machines

        public void DeleteMachines()
        {
            var machines = Repository.Machines.FindAll();
            var machinesToDelete = new ConcurrentBag<MachineResource>();
            foreach (var machine in machines)
            {
                if (machine.Name.Contains("Machine-CR-"))
                {
                    machinesToDelete.Add(machine);
                }
            }
            Parallel.ForEach(machinesToDelete, (machine, state, i) =>
            {
                Repository.Machines.Delete(machine);
            });
        }

        public void CreateMachines(int numberOfRecords)
        {
            var currentCount = Repository.Machines.FindAll().Count();

            var machineResources = new ConcurrentBag<MachineResource>();
            Parallel.For(currentCount, numberOfRecords, i =>
            {
                machineResources.Add(CreateMachine(i));
            });
            //for (var x = currentCount; x < numberOfRecords; x++)
            //    machineResources.Add(CreateMachine(x));
            
            Parallel.ForEach(machineResources, (machineResource, state, i) =>
            {
                Repository.Machines.Create(machineResource);
            });
            //for (var x = currentCount; x < numberOfRecords; x++)
            //    CreateMachine(x);
        }

        private MachineResource CreateMachine(int prefix)
        {
            var machine = new MachineResource()
            {
                Name = "Machine-CR-" + Guid.NewGuid().ToString().Substring(0, 8) + "-" + prefix.ToString("000"),
                Endpoint = new CloudRegionEndpointResource(),
            };

            // Spread across random environments that we've already created with SeaMonkey.
            var environments = Repository.Environments.GetAll().Where(e => e.Name.Contains("Env-")).ToList();
            var numberOfEnvironments = environments.Count;
            var env1 = environments[Program.Rnd.Next(0, numberOfEnvironments)];
            var env2 = environments[Program.Rnd.Next(0, numberOfEnvironments)];
            var env3 = environments[Program.Rnd.Next(0, numberOfEnvironments)];
            var env4 = environments[Program.Rnd.Next(0, numberOfEnvironments)];
            var env5 = environments[Program.Rnd.Next(0, numberOfEnvironments)];
            machine.EnvironmentIds.Add(env1.Id);
            if (!machine.EnvironmentIds.Contains(env2.Id))
                machine.EnvironmentIds.Add(env2.Id);
            if (!machine.EnvironmentIds.Contains(env3.Id))
                machine.EnvironmentIds.Add(env3.Id);
            if (!machine.EnvironmentIds.Contains(env4.Id))
                machine.EnvironmentIds.Add(env4.Id);
            if (!machine.EnvironmentIds.Contains(env5.Id))
                machine.EnvironmentIds.Add(env5.Id);

            var rolesPerMachine = RolesPerMachine.Get();
            machine.Roles.Add("cloud-region"); // All machines get this role.
            for (int i = 0; i < rolesPerMachine; i++)
            {
                if (i < this.PossibleRoles.Length)
                    machine.Roles.Add(this.PossibleRoles[i]);
                else // Fallback in case PossibleRoles doesn't have enough values based on the LinearProbability (shouldn't happen if they are in sync).
                    machine.Roles.Add(this.PossibleRoles[0]);
            }

            return machine;

            //return Repository.Machines.Create(machine);
        }

        private EnvironmentResource[] CreateEnvironments(int prefix)
        {
            var envs = new EnvironmentResource[EnvironmentsPerGroup.Get()];
            Enumerable.Range(1, envs.Length)
                .AsParallel()
                .ForAll(e =>
                {
                    var name = $"Env-{prefix:000}-{e}";
                    var envRes = Repository.Environments.FindByName(name);
                    envs[e - 1] = envRes ?? Repository.Environments.Create(new EnvironmentResource()
                    {
                        Name = name
                    });
                });
            return envs;
        }

        #endregion
    }
}
