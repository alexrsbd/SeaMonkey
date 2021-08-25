using System;
using Octopus.Client;
using Octopus.Shared.Internals.Options;
using SeaMonkey.Monkeys;
using SeaMonkey.ProbabilitySets;
using Serilog;

namespace SeaMonkey
{
    internal class Program
    {
        public static readonly Random Rnd = new Random(235346798);

        private static void Main(string[] args)
        {
            var server = string.Empty;
            var apiKey = string.Empty;
            var runSetupMonkey = false;
            var runTenantMonkey = false;
            var runDeployMonkey = false;
            var runRunbookRunMonkey = false;
            var runConfigurationMonkey = false;
            var runInfrastructureMonkey = false;
            var runLibraryMonkey = false;
            var runVariablesMonkey = false;

            try
            {
                var options = new OptionSet();
                options.Add<string>("server=", "The Octopus Server URI, E.g. http://localhost:8065", e => server = e);
                options.Add<string>("apiKey=", "The Octopus API key, E.g. API-1234", e => apiKey = e);
                options.Add<string>("runSetupMonkey", "", e => runSetupMonkey = true);
                options.Add<string>("runTenantMonkey", "", e => runTenantMonkey = true);
                options.Add<string>("runDeployMonkey", "", e => runDeployMonkey = true);
                options.Add<string>("runRunbookRunMonkey", "", e => runRunbookRunMonkey = true);
                options.Add<string>("runConfigurationMonkey", "", e => runConfigurationMonkey = true);
                options.Add<string>("runInfrastructureMonkey", "", e => runInfrastructureMonkey = true);
                options.Add<string>("runLibraryMonkey", "", e => runLibraryMonkey = true);
                options.Add<string>("runVariablesMonkey", "", e => runVariablesMonkey = true);

                if (args.Length == 0)
                    throw new ApplicationException(
                        "No arguments specified. Please provide a server, apiKey and some monkeys you wish to run. E.g. SeaMonkey.exe --server=http://localhost:8065 --apiKey=API-1234 --runConfigurationMonkey --runLibraryMonkey");

                options.Parse(args);

                if (string.IsNullOrWhiteSpace(server))
                    throw new ApplicationException(
                        "No server specified. Please use the --server parameter to specify an Octopus server instance.");
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new ApplicationException(
                        "No apiKey specified. Please use the --apiKey parameter to specify an Octopus API key.");

                var atLeastOneMonkeySpecified = runSetupMonkey || runTenantMonkey || runDeployMonkey || runRunbookRunMonkey ||
                                                runConfigurationMonkey || runInfrastructureMonkey || runLibraryMonkey ||
                                                runVariablesMonkey;
                if (!atLeastOneMonkeySpecified)
                    throw new ApplicationException(
                        "No monkeys were specified. Please use one of the following flags to run a monkey: --runSetupMonkey --runTenantMonkey --runDeployMonkey --runConfigurationMonkey --runInfrastructureMonkey --runLibraryMonkey --runVariablesMonkey");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.Message);
                Console.ResetColor();
                return;
            }

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.LiterateConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint(server, apiKey);
                var repository = new OctopusRepository(endpoint);
                RunMonkeys(repository,
                    runSetupMonkey,
                    runDeployMonkey,
                    runRunbookRunMonkey,
                    runConfigurationMonkey,
                    runInfrastructureMonkey,
                    runLibraryMonkey,
                    runTenantMonkey,
                    runVariablesMonkey);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OOPS");
            }
            finally
            {
                Console.WriteLine("Done. Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void RunMonkeys(OctopusRepository repository,
            bool runSetupMonkey,
            bool runDeployMonkey,
            bool runRunbookRunMonkey,
            bool runConfigurationMonkey,
            bool runInfrastructureMonkey,
            bool runLibraryMonkey,
            bool runTenantMonkey, 
            bool runVariablesMonkey)
        {
            Console.WriteLine("Starting monkey business...");

            if (runSetupMonkey)
            {
                Console.WriteLine("Running setup monkey...");
                new SetupMonkey(repository)
                {
                    StepsPerProject = new LinearProbability(1, 3)
                }.CreateProjectGroups(10);
            }

            if (runTenantMonkey)
            {
                new TenantMonkey(repository).Create(50);
            }

            if (runInfrastructureMonkey)
            {
                Console.WriteLine("Running infrastructure monkey...");
                new InfrastructureMonkey(repository)
                    .CreateRecords(7, 7, 7, 70, 2, 2);
            }

            if (runDeployMonkey)
            {
                Console.WriteLine("Running deploy monkey...");
                //new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 5000);
                new DeployMonkey(repository)
                    .RunForAllProjects(maxNumberOfDeployments: 100);
            }

            if (runRunbookRunMonkey)
            {
                Console.WriteLine("Running runbook run monkey...");
                new RunbookRunMonkey(repository)
                    .RunForAllRunbooks(maxNumberOfRunbookRuns: 100);
            }

            if (runConfigurationMonkey)
            {
                Console.WriteLine("Running configuration monkey...");
                new ConfigurationMonkey(repository)
                    .CreateRecords(70, 70, 70, 70);
            }

            if (runLibraryMonkey)
            {
                Console.WriteLine("Running library monkey...");
                new LibraryMonkey(repository)
                    .CreateRecords(70, 70, 10, 3, 70, 50);
            }

            // ReSharper disable once InvertIf
            if (runVariablesMonkey)
            {
                Console.WriteLine("Running variables monkey...");
                new VariablesMonkey(repository)
                    .CreateVariables(3, 10, 50, 100, 200);
                    //.CleanupVariables();
            }
        }
    }
}
