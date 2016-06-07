using System;
using Octopus.Client;
using SeaMonkey.Monkeys;
using Serilog;

namespace SeaMonkey
{
    class Program
    {
        public static readonly Random Rnd = new Random(235346798);


        static void Main()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint("http://localhost/loadtest", "API-OZGIAAFKSPBEQQHVMFI2NO0BZLG");
                var repository = new OctopusRepository(endpoint);
                new SetupMonkey(repository)
                    .Run(70);

                new DeployMonkey(repository)
                    .Run("Lots of Channels, Releases and Deployments", 3000);
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
