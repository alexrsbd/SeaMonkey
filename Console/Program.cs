using System;
using System.Threading;
using System.Threading.Tasks;
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

            //const string server = "http://localhost:8065/";
            //const string apikey = "API-HM8KZT6UKLBUGU0IBVDAUJBSWG";
            //const string server = "http://localhost";
            //const string apikey = "API-PFTW3YDKOF05TCCXQAXXO7DANEY";
            const string server = "http://localhost:8065";
            const string apikey = "API-GCCFRMSJ53TA9S9RN3SPW2UOPA8";

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint(server, apikey);
                var repository = new OctopusRepository(endpoint);

                //new SetupMonkey(repository).CreateTenants(500);
                //new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 5000);
                //new SetupMonkey(repository)
                //{
                //    StepsPerProject = new LinearProbability(1,3)
                //}.CreateProjectGroups(10);
                //new DeployMonkey(repository).RunForAllProjects(maxNumberOfDeployments:100);
                //new ConfigurationMonkey(repository)
                //    .CreateRecords(70, 70, 70);
                new InfrastructureMonkey(repository)
                    .CreateRecords(70, 70, 70);

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
