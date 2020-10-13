using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Octopus.Client;
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

        public IntProbability ProjectsPerGroup { get; set; } = new LinearProbability(10, 20);
        public IntProbability ExtraChannelsPerProject { get; set; } = new DiscretProbability(0, 1, 1, 5);
        public IntProbability EnvironmentsPerGroup { get; set; } = new FibonacciProbability();

        public void CreateProjectGroups(int numberOfGroups)
        {
            var machines = GetMachines();
            var currentCount = Repository.ProjectGroups.FindAll().Count();
            for (var x = currentCount; x <= numberOfGroups; x++)
                Create(x, machines);
        }

        private void Create(int id, IReadOnlyList<MachineResource> machines)
        {
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
            Enumerable.Range(1, numberOfProjects)
                .AsParallel()
                .ForAll(p =>
                    {
                        var project = CreateProject(group, lifecycle, $"-{prefix:000}-{p:00}");
                        UpdateDeploymentProcess(project);
                        CreateChannels(project, lifecycle);
                        SetVariables(project);
                        Log.Information("Created project {name}", project.Name);
                    }
                );
        }

        private void CreateChannels(ProjectResource project, LifecycleResource lifecycle)
        {
            var numberOfExtraChannels = ExtraChannelsPerProject.Get();

            Enumerable.Range(1, numberOfExtraChannels)
                .AsParallel()
                .ForAll(p =>
                    Repository.Channels.Create(new ChannelResource()
                    {
                        LifecycleId = lifecycle.Id,
                        ProjectId = project.Id,
                        Name = "Channel " + p.ToString("000"),
                        Rules = new List<ChannelVersionRuleResource>(),
                        IsDefault = false
                    })
                );
        }

        private EnvironmentResource[] CreateEnvironments(int prefix, IReadOnlyList<MachineResource> machines)
        {
            var envs = new EnvironmentResource[EnvironmentsPerGroup.Get()];
            Enumerable.Range(1, envs.Length)
                .AsParallel()
                .ForAll(e =>
                {
                    var name = $"Env-{prefix:000}-{e}";
                    var envRes = Repository.Environments.FindByName(name);
                    envs[e - 1] = envRes ?? Repository.Environments.Create(new EnvironmentResource()
                    {
                        Name = name
                    });
                });

            lock(this)
            {
                foreach (var env in envs)
                {
                    if (machines.Any())
                    {
                        var machine = machines[Program.Rnd.Next(0, machines.Count)];
                        Repository.Machines.Refresh(machine);
                        machine.EnvironmentIds.Add(env.Id);
                        Repository.Machines.Modify(machine);
                    }
                }
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
            var project = Repository.Projects.Create(new ProjectResource()
            {
                Name = "Project" + postfix,
                Description = @"Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?

Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?",
                ProjectGroupId = group.Id,
                LifecycleId = lifecycle.Id,
            });

            //try
            //{
            //    using (var ms = new MemoryStream(CreateLogo(project.Name, "monsterid")))
            //        Repository.Projects.SetLogo(project, project.Name + ".png", ms);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Failed to create logo for {project.Name}", ex);
            //}

            return project;
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