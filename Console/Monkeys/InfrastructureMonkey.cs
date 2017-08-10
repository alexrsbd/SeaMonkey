using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using System;
using Octopus.Client.Model.Accounts;

namespace SeaMonkey.Monkeys
{
    public class InfrastructureMonkey : Monkey
    {
        public InfrastructureMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void CreateRecords(int numberOfMachinePolicies, int numberOfProxies, int numberOfUsernamePasswords)
        {
            CreateMachinePolicies(numberOfMachinePolicies);
            CreateProxies(numberOfProxies);
            CreateUsernamePasswordAccounts(numberOfUsernamePasswords);
        }

        #region MachinePolicies

        public void CreateMachinePolicies(int numberOfRecords)
        {
            var currentCount = Repository.MachinePolicies.FindAll().Count();
            for (var x = currentCount; x <= numberOfRecords; x++)
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
            for (var x = currentCount; x <= numberOfRecords; x++)
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
            for (var x = currentCount; x <= numberOfRecords; x++)
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
    }
}
