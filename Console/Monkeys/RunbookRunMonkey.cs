using System;
using System.Linq;
using System.Threading;
using Octopus.Client;
using Octopus.Client.Model;
using SeaMonkey.ProbabilitySets;
using Serilog;
using Serilog.Events;

namespace SeaMonkey.Monkeys
{
    public class RunbookRunMonkey : Monkey
    {
        private readonly Random _rnd = new Random(235346798);
        private BooleanProbability ChanceOfAProcessChangeOnNewSnapshot { get; set; } = new BooleanProbability(0); // Enable selectively for debugging purposes. E.g. 0.75

        public RunbookRunMonkey(OctopusRepository repository) : base(repository)
        { }

        public class ProjectRunbookInfo
        {
            public ProjectResource Project { get; set; }
            public RunbookResource Runbook { get; set; }
            public ReferenceCollection EnvironmentIds { get; set; }
            public RunbookProcessResource RunbookProcess { get; set; }
            public RunbookSnapshotResource LatestSnapshot { get; set; }
        }

        public void RunForAllRunbooks(TimeSpan delayBetween = default(TimeSpan), int maxNumberOfRunbookRuns = int.MaxValue)
        {
            RunFor("All Runbooks", () => (prj, env) => true, delayBetween, maxNumberOfRunbookRuns);
        }

        private void RunFor(string description,
            Func<Func<ProjectRunbookInfo, string, bool>> filterFactory, 
            TimeSpan delayBetween = default(TimeSpan),
            int maxNumberOfRunbookRuns = int.MaxValue)
        {
            var projectRunbookInfos = GetProjectRunbookInfos();
            var projectRunbookEnvironmentPairs = (from p in projectRunbookInfos
                                           from e in p.EnvironmentIds
                                           select new
                                           {
                                               ProjectInfo = p,
                                               EnvironmentId = e,
                                           }).ToArray();

            for (var cnt = 1; cnt <= maxNumberOfRunbookRuns; cnt++)
            {
                var filter = filterFactory();
                var filteredItems = projectRunbookEnvironmentPairs
                    .Where(e => filter(e.ProjectInfo, e.EnvironmentId))
                    .ToArray();
                var item = filteredItems[_rnd.Next(0, filteredItems.Length)];

                CreateSnapshot(item.ProjectInfo);
                CreateRunbookRun(item.ProjectInfo, item.EnvironmentId);

                Log.Write(cnt % 10 == 0 ? LogEventLevel.Information : LogEventLevel.Verbose, "{description}: {n} runs", description, cnt);
                Thread.Sleep(delayBetween);
            }
        }

        private ProjectRunbookInfo[] GetProjectRunbookInfos()
        {
            var projectsLookup = Repository.Projects
                .GetAll()
                .ToDictionary(p => p.Id);
            var runbooks = projectsLookup                .SelectMany(p => Repository.Projects.GetAllRunbooks(p.Value))
                .ToArray();
            var environments = Repository.Environments.GetAll();

            var processes = runbooks
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Select(r => Repository.RunbookProcesses.Get(r.RunbookProcessId))
                .ToArray();

            var q = from r in runbooks
                    select new ProjectRunbookInfo
                    {
                        Project = projectsLookup[r.ProjectId],
                        Runbook = r,
                        EnvironmentIds = new ReferenceCollection(environments.Select(x => x.Id)),
                        RunbookProcess = processes.First(rp => rp.RunbookId == r.Id)
                    };

            return q.ToArray();
        }

        private void CreateSnapshot(ProjectRunbookInfo projectRunbookInfo)
        {
            if (ChanceOfAProcessChangeOnNewSnapshot.Get())
                projectRunbookInfo.RunbookProcess = UpdateRunbookProcess(projectRunbookInfo.Runbook);

            var snapshot = new RunbookSnapshotResource()
            {
                ProjectId = projectRunbookInfo.Project.Id,
                RunbookId = projectRunbookInfo.Runbook.Id,
                Name = $"Snapshot-{Guid.NewGuid()}",
                SelectedPackages = projectRunbookInfo.RunbookProcess
                    .Steps
                    .SelectMany(s => s.Actions)
                    .Where(a => a.Properties.ContainsKey("Octopus.Action.Package.NuGetPackageId"))
                    .Select(a => new SelectedPackage(a.Name, "3.2.4"))
                    .ToList()
            };
            projectRunbookInfo.LatestSnapshot = Repository.RunbookSnapshots.Create(snapshot);
        }

        private void CreateRunbookRun(ProjectRunbookInfo projectRunbookInfo, string environmentId)
        {
            Repository.RunbookRuns.Create(new RunbookRunResource()
            {
                ProjectId = projectRunbookInfo.Project.Id,
                RunbookId = projectRunbookInfo.Runbook.Id,
                RunbookSnapshotId = projectRunbookInfo.LatestSnapshot.Id,
                EnvironmentId = environmentId,
            });
        }
    }
}