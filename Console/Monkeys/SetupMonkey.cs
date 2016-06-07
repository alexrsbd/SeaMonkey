using System.Collections.Generic;
using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using SeaMonkey.ProbabilitySets;
using Serilog;

namespace SeaMonkey.Monkeys
{

    public class SetupMonkey : Monkey
    {

        public SetupMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public IntProbability ProjectsPerGroup { get; set; } = new LinearProbability(5, 10);
        public IntProbability ExtraChannelsPerProject { get; set; } = new DiscretProbability(0, 1, 1, 5);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();

        public void Run(int upToNumberOfGroups)
        {
            var numberOfGroups = Repository.ProjectGroups.FindAll().Count();
            for (var x = numberOfGroups; x <= upToNumberOfGroups; x++)
                Create(x);
        }

        private void Create(int id)
        {
            var machines = GetMachines();
            var envs = CreateEnvironments(id, machines);
            var lc = CreateLifecycle(id, envs);
            var group = CreateProjectGroup(id);
            CreateProjects(id, group, lc);
        }


        private ProjectGroupResource CreateProjectGroup(int prefix)
        {
            return
                Repository.ProjectGroups.Create(new ProjectGroupResource()
                {
                    Name = "Group-" + prefix.ToString("000")
                });
        }


        private void CreateProjects(int prefix, ProjectGroupResource group, LifecycleResource lifecycle)
        {
            var numberOfProjects = ProjectsPerGroup.Get();
            Log.Information("Creating {n} projects for {group}", numberOfProjects, group.Name);
            for (var p = 1; p <= numberOfProjects; p++)
            {
                var project = CreateProject(group, lifecycle, $"-{prefix:000}-{p:00}");
                UpdateDeploymentProcess(project);
                CreateChannels(project, lifecycle);
                SetVariables(project);
                Log.Information("Created project {name}", project.Name);
            }
        }



        private void CreateChannels(ProjectResource project, LifecycleResource lifecycle)
        {
            var numberOfExtraChannels = ExtraChannelsPerProject.Get();
            for (var p = 1; p <= numberOfExtraChannels; p++)
            {
                Repository.Channels.Create(new ChannelResource()
                {
                    LifecycleId = lifecycle.Id,
                    ProjectId = project.Id,
                    Name = "Channel " + p.ToString("000"),
                    Rules = new List<ChannelVersionRuleResource>(),
                    IsDefault = false
                });
            }
        }

        private EnvironmentResource[] CreateEnvironments(int prefix, IReadOnlyList<MachineResource> machines)
        {
            var envs = new EnvironmentResource[EnvironmentsPerGroup.Get()];
            for (int e = 1; e <= envs.Length; e++)
            {
                envs[e - 1] = Repository.Environments.Create(new EnvironmentResource()
                {
                    Name = $"Env-{prefix:000}-{e}"
                });
            }
            foreach (var env in envs)
            {
                var machine = machines[Program.Rnd.Next(0, machines.Count)];
                Repository.Machines.Refresh(machine);
                machine.EnvironmentIds.Add(env.Id);
                Repository.Machines.Modify(machine);
            }
            return envs;
        }


        private LifecycleResource CreateLifecycle(int id, IEnumerable<EnvironmentResource> environments)
        {
            var lc = new LifecycleResource()
            {
                Name = "Life" + id.ToString("000"),
            };
            lc.Phases.Add(new PhaseResource()
            {
                Name = "AllTheEnvs",
                OptionalDeploymentTargets = new ReferenceCollection(environments.Select(ef => ef.Id))
            });
            return Repository.Lifecycles.Create(lc);
        }

        private ProjectResource CreateProject(ProjectGroupResource group, LifecycleResource lifecycle, string postfix)
        {
            return Repository.Projects.Create(new ProjectResource()
            {
                Name = "Project" + postfix,
                ProjectGroupId = group.Id,
                LifecycleId = lifecycle.Id
            });
        }


    }
}