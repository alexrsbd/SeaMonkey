using System;
using Octopus.Client;
using SeaMonkey.Monkeys;
using SeaMonkey.ProbabilitySets;
using Serilog;

namespace SeaMonkey
{
    class Program
    {
        public static readonly Random Rnd = new Random(235346798);

        static void Main(string[] args)
        {
            Random rnd = new Random();

            const string server = "http://localhost:8065/";
            const string apikey = "API-HM8KZT6UKLBUGU0IBVDAUJBSWG";
            //const string server = "http://localhost:8065";
            //const string apikey = "API-GCCFRMSJ53TA9S9RN3SPW2UOPA8";
            const bool runSetupMonkey = true;
            const bool runDeployMonkey = true;
            const bool runConfigurationMonkey = false;
            const bool runInfrastructureMonkey = false;
            const bool runLibraryMonkey = false;

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint(server, apikey);
                var repository = new OctopusRepository(endpoint);
                RunMonkeys(repository,
                    runSetupMonkey,
                    runDeployMonkey,
                    runConfigurationMonkey,
                    runInfrastructureMonkey,
                    runLibraryMonkey);
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

        static void RunMonkeys(OctopusRepository repository,
            bool runSetupMonkey,
            bool runDeployMonkey,
            bool runConfigurationMonkey,
            bool runInfrastructureMonkey,
            bool runLibraryMonkey)
        {
            Console.WriteLine("Starting monkey business...");

            if (runSetupMonkey)
            {
                Console.WriteLine("Running setup monkey...");
                //new SetupMonkey(repository).CreateTenants(500);
                new SetupMonkey(repository)
                {
                    StepsPerProject = new LinearProbability(1, 3)
                }.CreateProjectGroups(10);
            }

            if (runDeployMonkey)
            {
                Console.WriteLine("Running deploy monkey...");
                //new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 5000);
                new DeployMonkey(repository)
                    .RunForAllProjects(maxNumberOfDeployments: 100);
            }

            if (runConfigurationMonkey)
            {
                Console.WriteLine("Running configuration monkey...");
                new ConfigurationMonkey(repository)
                    .CreateRecords(70, 70, 70, 70);
            }

            if (runInfrastructureMonkey)
            {
                Console.WriteLine("Running infrastructure monkey...");
                new InfrastructureMonkey(repository)
                    .CreateRecords(70, 70, 70, 70);
            }

            if (runLibraryMonkey)
            {
                Console.WriteLine("Running library monkey...");
                new LibraryMonkey(repository)
                    .CreateRecords(70, 70, 70, 70);
            }

        }
    }
}
