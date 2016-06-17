using System;
using Octopus.Client;
using SeaMonkey.Monkeys;
using Serilog;

namespace SeaMonkey
{
    class Program
    {
        public static readonly Random Rnd = new Random(235346798);


        static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint(args[0], args[1]);
                var repository = new OctopusRepository(endpoint);

                new SetupMonkey(repository).CreateProjectGroups(70);
                new SetupMonkey(repository).CreateTenants(500);
                new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 5000);
                new DeployMonkey(repository).RunForAllProjects(5000);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OOPS");
            }
            finally
            {
                Console.WriteLine("Done. Press any key to exit");
                Console.ReadLine();
            }
        }
    }
}
