using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Octopus.Client;
using Octopus.Client.Model;
using SeaMonkey.ProbabilitySets;
using Serilog;

namespace SeaMonkey.Monkeys
{

    public class SetupMonkey : Monkey
    {
        public const string TenantedGroupName = "A Tenanted Group";

        public SetupMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public IntProbability ProjectsPerGroup { get; set; } = new LinearProbability(5, 10);
        public IntProbability ExtraChannelsPerProject { get; set; } = new DiscretProbability(0, 1, 1, 5);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();

        public void CreateProjectGroups(int numberOfGroups)
        {
            var currentCount = Repository.ProjectGroups.FindAll().Count();
            for (var x = currentCount; x <= numberOfGroups; x++)
                Create(x);
        }

        public void CreateTenants(int numberOfTenants)
        {
            var tenantedGroup = Repository.ProjectGroups.FindByName(TenantedGroupName);
            if (tenantedGroup == null)
            {
                tenantedGroup = Repository.ProjectGroups.FindOne(g => g.Name.StartsWith("Group-"));
                tenantedGroup.Name = TenantedGroupName;
                Repository.ProjectGroups.Modify(tenantedGroup);
            }
            var projects = Repository.ProjectGroups.GetProjects(tenantedGroup);
            var currentTenantCount = Repository.Tenants.FindAll().Count;
            var lifecycles = Repository.Lifecycles.FindAll();
            var environments = lifecycles
                .Where(l => projects.Any(p => p.LifecycleId == l.Id))
                .SelectMany(l => l.Phases[0].OptionalDeploymentTargets)
                .ToArray();

            for (var x = currentTenantCount + 1; x <= numberOfTenants; x++)
            {
                var tenant = Repository.Tenants.Create(new TenantResource()
                {
                    Name = "Tenant-" + x.ToString("000"),
                    ProjectEnvironments = projects.ToDictionary(p => p.Id, p => new ReferenceCollection(environments))
                });

                using (var client = new HttpClient())
                {
                    var img = client.GetByteArrayAsync("https://robohash.org/" + tenant.Name).Result;
                    using (var ms = new MemoryStream(img))
                        Repository.Tenants.SetLogo(tenant, tenant.Name + ".png", ms);
                }
            }

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
                SetProjectImage(project);
                Log.Information("Created project {name}", project.Name);
            }
        }

        private void SetProjectImage(ProjectResource project)
        {
            using (var client = new HttpClient())
            {
                var img = client.GetByteArrayAsync("https://api.adorable.io/avatars/400/" + project.Name).Result;
                using (var ms = new MemoryStream(img))
                    Repository.Projects.SetLogo(project, project.Name + ".png", ms);
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
                LifecycleId = lifecycle.Id,
                ProjectConnectivityPolicy = new ProjectConnectivityPolicy()
                {
                    SkipMachineBehavior = SkipMachineBehavior.SkipUnavailableMachines
                }
            });
        }


    }
}