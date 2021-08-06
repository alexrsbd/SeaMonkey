﻿using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Model.Accounts;
using Octopus.Client.Model.Endpoints;
using SeaMonkey.ProbabilitySets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Serilog;

namespace SeaMonkey.Monkeys
{
    public class InfrastructureMonkey : Monkey
    {
        private IntProbability RolesPerMachine { get; set; } = new LinearProbability(0, 4);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();
        private readonly string[] PossibleRoles = new string[] {
            "Rick",
            "Morty",
            "Mr. Meeseeks",
            "Roy's Carpet Store",
            "InstallStuff", // <- need to have this role somewhere for setup.
        };
        const string WorkerPoolPrefix = "GarblovianWorkerPool-";
        const string WorkerPrefix = "RickAndMortyWorker-";

        public InfrastructureMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void CreateRecords(int numberOfMachinePolicies,
            int numberOfProxies,
            int numberOfUsernamePasswords,
            int numberOfMachines,
            int numberOfWorkerPools,
            int numberOfWorkersPerPool)
        {
            CreateEnvironments(0);
            CreateMachinePolicies(numberOfMachinePolicies);
            CreateProxies(numberOfProxies);
            CreateUsernamePasswordAccounts(numberOfUsernamePasswords);
            CreateMachines(numberOfMachines);
            CreateWorkersPoolsAndWorkers(numberOfWorkerPools, numberOfWorkersPerPool);
        }

        #region MachinePolicies

        public void CreateMachinePolicies(int numberOfRecords)
        {
            Log.Information("Creating {n} machine policies", numberOfRecords);
            var currentCount = Repository.MachinePolicies.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateMachinePolicy(i));
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

                    },
                    PollingRequestQueueTimeout = new TimeSpan(1,0,0),
                    PollingRequestMaximumMessageProcessingTimeout = new TimeSpan(1, 0, 0),
                    ConnectionRetrySleepInterval = new TimeSpan(1, 0, 0),
                    ConnectionRetryCountLimit = 3,
                    ConnectionRetryTimeLimit = new TimeSpan(1, 0, 0),
                    ConnectionConnectTimeout = new TimeSpan(1, 0, 0),
            });
        }

        #endregion

        #region Proxies

        public void CreateProxies(int numberOfRecords)
        {
            Log.Information("Creating {n} proxies", numberOfRecords);
            var currentCount = Repository.Proxies.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateProxy(i));
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
            Log.Information("Creating {n} username/password accounts", numberOfRecords);
            var currentCount = Repository.Accounts.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateUsernamePasswordAccount(i));
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
            };
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
            Parallel.ForEach(machineResources, (machineResource, state, i) =>
            {
                Repository.Machines.Create(machineResource);
            });
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

        #region Workers

        private void CreateWorkersPoolsAndWorkers(int numberOfWorkerPools, int numberOfWorkersPerPool)
        {
            // Setup worker pools first.
            var currentPoolCount = Repository.WorkerPools.GetAll().Count(e => e.Name.Contains(WorkerPoolPrefix));
            var poolResources = new ConcurrentBag<WorkerPoolResource>();
            Parallel.For(currentPoolCount, numberOfWorkerPools, i =>
            {
                poolResources.Add(CreateWorkerPool(i));
            });
            Parallel.ForEach(poolResources, (poolResource, state, i) =>
            {
                Repository.WorkerPools.Create(poolResource);
            });

            // Setup workers for each pool.
            var pools = Repository.WorkerPools.GetAll().Where(e => e.Name.Contains(WorkerPoolPrefix)).ToList();
            foreach (var pool in pools)
            {
                var currentWorkerCount = Repository.Workers.FindAll().Count(e => e.WorkerPoolIds.Contains(pool.Id) && e.Name.Contains(WorkerPrefix));
                var workerResources = new ConcurrentBag<WorkerResource>();
                Parallel.For(currentWorkerCount, numberOfWorkersPerPool, i =>
                {
                    workerResources.Add(CreateWorker(pool.Id, i));
                });
                Parallel.ForEach(workerResources, (workerResource, state, i) =>
                {
                    Repository.Workers.Create(workerResource);
                });
            }
        }

        private static WorkerPoolResource CreateWorkerPool(int prefix)
        {
            var pool = new WorkerPoolResource()
            {
                Name = "GarblovianWorkerPool-" + Guid.NewGuid().ToString().Substring(0, 8) + "-" + prefix.ToString("000"),
            };
            return pool;
        }

        private static WorkerResource CreateWorker(string workerPoolId, int prefix)
        {
            var worker = new WorkerResource()
            {
                Name = WorkerPrefix + Guid.NewGuid().ToString().Substring(0, 8) + "-" + prefix.ToString("000"),
                WorkerPoolIds = new ReferenceCollection(workerPoolId),
                Endpoint = new ListeningTentacleEndpointResource()
                {
                    Uri = "https://localhost:10940", // TODO: pass these as optional params?
                    Thumbprint = "62D0D4851EE3B94C5B4D221F5583577AB9DEC015", // TODO: pass these as optional params?
                },
            };
            return worker;
        }

        #endregion
    }
}
