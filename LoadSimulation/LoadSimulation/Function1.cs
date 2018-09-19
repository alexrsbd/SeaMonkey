using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Octopus.Client;
using Serilog;

namespace LoadSimulation
{
    public static class Function1
    {
        static Function1()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Seq("", apiKey: "")
                .Enrich.WithProperty("Application", "CattleClassTesting")
                .MinimumLevel.Verbose()
                .CreateLogger();
        }


        [FunctionName("Run800_1")]
        public static Task Run800_1([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(800);

        [FunctionName("Run800_2")]
        public static Task Run800_2([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(800);

        [FunctionName("Run800_3")] 
        public static Task Run800_3([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(800);


        [FunctionName("Run830_1")]
        public static Task Run830_1([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(830);

        [FunctionName("Run830_2")]
        public static Task Run830_2([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(830);

        [FunctionName("Run830_3")]
        public static Task Run830_3([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, TraceWriter log)
            => Run(830);

        [FunctionName("Run801")] public static Task Run801([TimerTrigger("0 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(801);
        [FunctionName("Run802")] public static Task Run802([TimerTrigger("2 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(802);
        [FunctionName("Run803")] public static Task Run803([TimerTrigger("4 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(803);
        [FunctionName("Run804")] public static Task Run804([TimerTrigger("6 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(804);
        [FunctionName("Run805")] public static Task Run805([TimerTrigger("8 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(805);
        [FunctionName("Run806")] public static Task Run806([TimerTrigger("10 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(806);
        [FunctionName("Run807")] public static Task Run807([TimerTrigger("12 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(807);
        [FunctionName("Run808")] public static Task Run808([TimerTrigger("14 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(808);
        [FunctionName("Run809")] public static Task Run809([TimerTrigger("16 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(809);
        [FunctionName("Run810")] public static Task Run810([TimerTrigger("18 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(810);
        [FunctionName("Run811")] public static Task Run811([TimerTrigger("20 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(811);
        [FunctionName("Run812")] public static Task Run812([TimerTrigger("22 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(812);
        [FunctionName("Run813")] public static Task Run813([TimerTrigger("24 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(813);
        [FunctionName("Run814")] public static Task Run814([TimerTrigger("26 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(814);
        [FunctionName("Run815")] public static Task Run815([TimerTrigger("28 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(815);
        [FunctionName("Run816")] public static Task Run816([TimerTrigger("30 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(816);
        [FunctionName("Run817")] public static Task Run817([TimerTrigger("32 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(817);
        [FunctionName("Run818")] public static Task Run818([TimerTrigger("34 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(818);
        [FunctionName("Run819")] public static Task Run819([TimerTrigger("36 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(819);
        [FunctionName("Run820")] public static Task Run820([TimerTrigger("38 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(820);
        [FunctionName("Run821")] public static Task Run821([TimerTrigger("40 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(821);
        [FunctionName("Run822")] public static Task Run822([TimerTrigger("42 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(822);
        [FunctionName("Run823")] public static Task Run823([TimerTrigger("44 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(823);
        [FunctionName("Run824")] public static Task Run824([TimerTrigger("46 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(824);
        [FunctionName("Run825")] public static Task Run825([TimerTrigger("48 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(825);
        [FunctionName("Run826")] public static Task Run826([TimerTrigger("50 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(826);
        [FunctionName("Run827")] public static Task Run827([TimerTrigger("52 * * * * *")] TimerInfo myTimer, TraceWriter log) => Run(827);





        public static async Task Run(int id)
        {
            var correlatedLog = Log.ForContext("CorrelationId", Guid.NewGuid())
                .ForContext("Id", id);
            try
            {
                var sw = Stopwatch.StartNew();
                var sw2 = Stopwatch.StartNew();

                var endpoint = new OctopusServerEndpoint("http://54.79.14.25/" + id.ToString("000"));
                var client = await OctopusAsyncClient.Create(endpoint);
                var repository = client.Repository;
                await repository.Users.SignIn("Admin", "ThePassword");
                LogOperation(correlatedLog, sw2, "Signin");

                var dashboard = await repository.Dashboards.GetDashboard();
                var latestRelease = dashboard.Items.OrderByDescending(d => d.StartTime).First();
                LogOperation(correlatedLog, sw2, "Dashboard");

                var summary = await repository.Environments.Summary();
                LogOperation(correlatedLog, sw2, "Summary");

                var project = await repository.Projects.Get(latestRelease.ProjectId);
                LogOperation(correlatedLog, sw2, "Project");

                var task = await repository.Tasks.Get(latestRelease.TaskId);
                LogOperation(correlatedLog, sw2, "Task");

                var logs = await repository.Tasks.GetDetails(task);
                LogOperation(correlatedLog, sw2, "TaskDetails");


                correlatedLog.Information("Ran user script against {id} in {ms}", id, sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
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