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

                new SetupMonkey(repository).Run(2,8);
                //new DeployMonkey(repository).RunForAllProjects(100);
                new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 10);
               // new DeployMonkey(repository).RunForAllProjects(1000);
               // new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 1000);
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
