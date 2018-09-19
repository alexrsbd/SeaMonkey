using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Client;
using Serilog;

namespace LoadSimulationConsole
{
    class Program
    {
        private static bool _running = true;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("", apiKey: "")
                .Enrich.WithProperty("Application", "CattleClassTesting")
                .MinimumLevel.Verbose()
                .CreateLogger();

           
            Run(800, 5);
            Run(830, 5);
            Thread.Sleep(1000);
            Run(800, 5);
            Run(830, 5);
            Thread.Sleep(1000);
            Run(800, 5);
            Run(830, 5);

            for (int x = 1; x < 27; x++)
                Run(800 + x, 10);


            Console.WriteLine("Running");
            Console.ReadLine();
            _running = false;
        }

        private static void Run(int id, int interval)
        {
            new Thread(() =>
            {
                var endpoint = new OctopusServerEndpoint("http://54.79.14.25/" + id.ToString("000"));
                var repository = new OctopusRepository(endpoint);
                repository.Users.SignIn("Admin", "ThePassword");


                while (_running)
                {
                    try
                    {
                        Console.WriteLine(id);
                        Run(repository, id);
                        Thread.Sleep(TimeSpan.FromSeconds(interval));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }
            }).Start();
        }

        static void Run(OctopusRepository repository, int id)
        {
            var correlatedLog = Log.ForContext("CorrelationId", Guid.NewGuid())
                .ForContext("Id", id);
            try
            {
                var sw = Stopwatch.StartNew();
                var sw2 = Stopwatch.StartNew();

                var dashboard = repository.Dashboards.GetDashboard();
                var latestRelease = dashboard.Items.OrderByDescending(d => d.StartTime).First();
                LogOperation(correlatedLog, sw2, "Dashboard");

                var summary = repository.Environments.Summary();
                LogOperation(correlatedLog, sw2, "Summary");

                var project = repository.Projects.Get(latestRelease.ProjectId);
                LogOperation(correlatedLog, sw2, "Project");

                var task = repository.Tasks.Get(latestRelease.TaskId);
                LogOperation(correlatedLog, sw2, "Task");

                var logs = repository.Tasks.GetDetails(task);
                LogOperation(correlatedLog, sw2, "TaskDetails");


                correlatedLog.Information("Ran user script against {id} in {ms}", id, sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                correlatedLog.Warning(e, "Exception running user script");
            }
        }

        private static void LogOperation(ILogger correlatedLog, Stopwatch sw2, string op)
        {
            correlatedLog.Verbose("Operation {op} took {ms}ms", op, sw2.ElapsedMilliseconds);
            sw2.Restart();
        }
    }
}