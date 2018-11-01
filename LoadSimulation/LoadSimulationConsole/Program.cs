using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Serialization;
using Serilog;

namespace LoadSimulationConsole
{
    class Program
    {
        private static bool _running = true;
        private static Semaphore _sem = new Semaphore(5, 5);
        private static Instance[] _instances;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("https://seq.octopushq.com", apiKey: "")
                .Enrich.WithProperty("Application", "HostedWindowsK8Testing")
                .MinimumLevel.Verbose()
                .CreateLogger();

            _instances = JsonSerialization.DeserializeObject<Instance[]>(File.ReadAllText(@"services.json"));

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 5; y++)
                    Run(y, 5);

                Thread.Sleep(1000);
            }

            for (int x = 1; x < 20; x++)
                Run(x, 10);


            Console.WriteLine("Running");
            Console.ReadLine();
            _running = false;
        }

        private static void Run(int id, int interval)
        {
            new Thread(() =>
            {
                var instance = _instances[id];
                Console.WriteLine("Starting " + instance.Machine);
                var endpoint = new OctopusServerEndpoint($"http://{instance.PublicIp}:81/");
                var repository = new OctopusRepository(endpoint);
                repository.Users.SignIn("Admin", "password");


                while (_running)
                {
                    try
                    {
                        Console.WriteLine(instance.Machine);
                        Run(repository, instance);
                        Thread.Sleep(TimeSpan.FromSeconds(interval));
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }
                }
            }).Start();
        }

        static void Run(OctopusRepository repository, Instance instance)
        {
            var correlatedLog = Log.ForContext("CorrelationId", Guid.NewGuid())
                .ForContext("Machine", instance.Machine);
            try
            {
                _sem.WaitOne();
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


                correlatedLog.Information("Ran user script against {Machine} in {ms}", instance.Machine, sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                correlatedLog.Warning(e, "Exception running user script");
            }
            finally
            {
                _sem.Release();
            }
        }

        private static void LogOperation(ILogger correlatedLog, Stopwatch sw2, string op)
        {
            //correlatedLog.Verbose("Operation {op} took {ms}ms", op, sw2.ElapsedMilliseconds);
            sw2.Restart();
        }
        
        
        class Instance
        {
            public string Machine { get; set; }
            public string PublicIp { get; set; }
            public string PrivateIp { get; set; }
        }

    }
}