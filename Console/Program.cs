using System;
using System.Threading;
using System.Threading.Tasks;
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
            Random rnd = new Random();

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.ColoredConsole()
                    .CreateLogger();

                var endpoint = new OctopusServerEndpoint(args[0], args[1]);
                var repository = new OctopusRepository(endpoint);

                //new SetupMonkey(repository).CreateProjectGroups(70);
                //new SetupMonkey(repository).CreateTenants(500);
                //new DeployMonkey(repository).RunForGroup(SetupMonkey.TenantedGroupName, 5000);
                //new DeployMonkey(repository).RunForAllProjects(5000);
                var group = repository.ProjectGroups.FindByName(SetupMonkey.TenantedGroupName);

                new DeployMonkey(repository)
                    .RunFor("Custom",
                        () =>
                        {
                            switch (rnd.Next(3))
                            {
                                case 0:
                                    return (prj, env, tenant) => prj.Project.ProjectGroupId != group.Id && prj.Project.Name != "A Super Duper Project";
                                case 1:
                                    return (prj, env, tenant) => prj.Project.ProjectGroupId == group.Id && prj.Project.Name != "A Super Duper Project";
                                default:
                                    return (prj, env, tenant) => prj.Project.Name == "A Super Duper Project";
                            }
                        }, TimeSpan.FromSeconds(5));
            
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
