using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Octopus.Client;
using Octopus.Client.Editors;
using Octopus.Client.Model;
using Polly;
using SeaMonkey.ProbabilitySets;
using Serilog;

namespace SeaMonkey.Monkeys
{
    public class SetupMonkey : Monkey
    {
        private static byte[] lastImage;
        public SetupMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public IntProbability ProjectsPerGroup { get; set; } = new LinearProbability(5, 10);
        public IntProbability ExtraChannelsPerProject { get; set; } = new DiscretProbability(0, 1, 1, 5);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();
        public IntProbability RunbooksPerProject { get; set; } = new LinearProbability(1, 5);

        public void CreateProjectGroups(int numberOfRecords)
        {
            Log.Information("Creating {n} project groups", numberOfRecords);

            var machines = GetMachines();
            var currentCount = Repository.ProjectGroups.FindAll().Count();

            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => Create(i, machines));
        }

        private void Create(int id, IReadOnlyList<MachineResource> machines)
        {
            var environments = CreateEnvironments(id, machines);
            var lifecycle = CreateLifecycle(id, environments);
            var projectGroup = CreateProjectGroup(id);
            CreateProjects(id, projectGroup, lifecycle);
        }

        private ProjectGroupResource CreateProjectGroup(int prefix)
        {
            return Repository.ProjectGroups.CreateOrModify("Group-" + prefix.ToString("000")).Instance;
        }

        private void CreateProjects(int prefix, ProjectGroupResource group, LifecycleResource lifecycle)
        {
            var numberOfProjects = ProjectsPerGroup.Get();
            Log.Information("Creating {n} projects for {group}", numberOfProjects, group.Name);
            Enumerable.Range(1, numberOfProjects)
                .ToList()
                .ForEach(p =>
                    {
                        var project = CreateProject(group, lifecycle, $"-{prefix:000}-{p:00}");
                        var process = UpdateDeploymentProcess(project);
                        CreateChannels(project, lifecycle);
                        CreateRunbooks(project);
                        SetVariables(project);
                        EnableArcIfPackageStepsExist(project, process);
                        Log.Information("Created project {name}", project.Name);
                    }
                );
        }

        private void EnableArcIfPackageStepsExist(ProjectResource project, DeploymentProcessResource process)
        {
            var firstPackageActionName = process.Steps
                .SelectMany(s => s.Actions)
                .Where(a => a.Properties.ContainsKey("Octopus.Action.Package.NuGetPackageId"))
                .Select(a => a.Name)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(firstPackageActionName))
                return;

            var defaultChannel = Repository.Projects.GetAllChannels(project).Single(x => x.IsDefault);
            project.ReleaseCreationStrategy.ReleaseCreationPackage = new DeploymentActionPackageResource(firstPackageActionName);
            project.ReleaseCreationStrategy.ChannelId = defaultChannel.Id;
            project.AutoCreateRelease = true;
            Repository.Projects.Modify(project);
        }

        private void CreateRunbooks(ProjectResource project)
        {
            var numberOfRunbooks = RunbooksPerProject.Get();
            Log.Information("Creating {n} runbooks for {project}", numberOfRunbooks, project.Name);
            var runbookEditor = new RunbookEditor(Repository.Runbooks, Repository.RunbookProcesses);
            Enumerable.Range(1, numberOfRunbooks)
                .ToList()
                .ForEach(i =>
                    {
                        runbookEditor.CreateOrModify(project, $"Runbook {project.Id} {i:000}", "");
                        var runbook = runbookEditor.Instance;
                        UpdateRunbookProcess(runbook);
                    }
                );
        }

        private void CreateChannels(ProjectResource project, LifecycleResource lifecycle)
        {
            var numberOfExtraChannels = ExtraChannelsPerProject.Get();
            Enumerable.Range(1, numberOfExtraChannels)
                .ToList()
                .ForEach(p =>
                    {
                        Repository.Channels
                            .CreateOrModify(project, "Channel " + p.ToString("000"), string.Empty)
                            .UsingLifecycle(lifecycle)
                            .ClearRules();
                    }
                );
        }

        private EnvironmentResource[] CreateEnvironments(int prefix, IReadOnlyList<MachineResource> machines)
        {
            var environments = new EnvironmentResource[EnvironmentsPerGroup.Get()];
            Enumerable.Range(1, environments.Length)
                .ToList()
                .ForEach(e =>
                {
                    var name = $"Env-{prefix:000}-{e}";
                    environments[e - 1] = Repository.Environments.CreateOrModify(name).Instance;
                });

            lock (this)
            {
                foreach (var env in environments)
                {
                    if (!machines.Any())
                        continue;

                    var machine = machines[Program.Rnd.Next(0, machines.Count)];
                    Repository.Machines.Refresh(machine);
                    machine.EnvironmentIds.Add(env.Id);
                    Repository.Machines.Modify(machine);
                }
            }
            return environments;
        }

        private LifecycleResource CreateLifecycle(int id, IEnumerable<EnvironmentResource> environments)
        {
            var lc = new LifecycleResource()
            {
                Name = "Life" + id.ToString("000"),
                ReleaseRetentionPolicy = new RetentionPeriod(3, RetentionUnit.Days),
                TentacleRetentionPolicy = new RetentionPeriod(3, RetentionUnit.Days),
            };
            lc.Phases.Add(new PhaseResource()
            {
                Name = "AllTheEnvs",
                OptionalDeploymentTargets = new ReferenceCollection(environments.Select(ef => ef.Id))
            });
            
            return Repository.Lifecycles.CreateOrModify(lc.Name).Instance;
        }

        private ProjectResource CreateProject(ProjectGroupResource group, LifecycleResource lifecycle, string postfix)
        {
            var description = @"Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?

Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?";

            var projectEditor = Repository.Projects.CreateOrModify("Project" + postfix, group, lifecycle, description, null);

            //try
            //{
            //    using (var ms = new MemoryStream(CreateLogo(project.Name, "monsterid")))
            //        Repository.Projects.SetLogo(project, project.Name + ".png", ms);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Failed to create logo for {project.Name}", ex);
            //}

            return projectEditor.Instance;
        }

        /// <summary>
        /// Type is from https://en.gravatar.com/site/implement/images/
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static byte[] CreateLogo(string name, string type = "retro")
        {
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(name))).Replace("-", "").ToLower();

            using (var client = new HttpClient())
            {
                byte[] image = lastImage;
                Policy
                    .Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    }, (exception, timeSpan) => image = lastImage)
                    .Execute(() =>
                    {
                        image = client
                                .GetByteArrayAsync($"https://www.gravatar.com/avatar/{hash}?s=256&d={type}&r=PG")
                                .Result;
                        lastImage = image;
                    });

                return image;
            }
        }
    }
}