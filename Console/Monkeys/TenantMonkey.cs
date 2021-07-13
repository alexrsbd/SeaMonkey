using System;
using System.Collections.Generic;
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
    public class TenantMonkey : Monkey
    {
        private static byte[] lastImage;

        public TenantMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public int MaxTenantedProjects = 10;

        public IntProbability ProjectsPerTenant { get; set; } = new LinearProbability(1, 10);

        public IntProbability EnvironmentsPerProjectLink { get; set; } = new LinearProbability(0, 4);


        public void Create(int numberOfRecords)
        {
            var projects = GetProjects();
            var lifecycleIndex = Repository.Lifecycles.FindAll().ToDictionary(t => t.Id);

            Log.Information("Creating {n} tenants", numberOfRecords);

            Enumerable.Range(1, numberOfRecords)
                .AsParallel()
                .ForAll(i =>
                    {
                        CreateTenant(projects, lifecycleIndex, i);

                        //try
                        //{
                        //    using (var ms = new MemoryStream(CreateLogo(project.Name, "monsterid")))
                        //        Repository.Projects.SetLogo(project, project.Name + ".png", ms);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine($"Failed to create logo for {project.Name}", ex);
                        //}
                    }
                );
        }


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

        List<ProjectResource> GetProjects()
        {
            var currentProjects = Repository.Projects.GetAll();

            var tenantableProjects = currentProjects.Where(t => t.TenantedDeploymentMode != TenantedDeploymentMode.Untenanted).ToList();
            if (tenantableProjects.Count >= MaxTenantedProjects)
                return tenantableProjects;

            var projectsAllowedToBecomeTenanted = currentProjects
                .Where(t => t.TenantedDeploymentMode == TenantedDeploymentMode.Untenanted)
                .TakeRandomSubset(MaxTenantedProjects - tenantableProjects.Count);

            return tenantableProjects.Concat(projectsAllowedToBecomeTenanted).ToList();
        }

        private void CreateTenant(List<ProjectResource> currentProjects, Dictionary<string, LifecycleResource> lifecycleIndex, int t)
        {
            var projects = currentProjects
                .TakeRandomSubset(ProjectsPerTenant.Get())
                .ToList();

            EnsureProjectsTenanted(projects);

            var tenantName = "Tenant " + t.ToString("000");

            var tenantBuilder = Repository.Tenants.CreateOrModify(tenantName);
            tenantBuilder.ClearProjects();
            tenantBuilder.Instance.ProjectEnvironments = projects.ToDictionary(p => p.Id, p => GetEnvironmentsForProject(p, lifecycleIndex));
            tenantBuilder.Save();
            Log.Information("Created tenant {name}", tenantName);
        }

        private void EnsureProjectsTenanted(List<ProjectResource> projects)
        {
            foreach (var untenantedProject in projects.Where(p => p.TenantedDeploymentMode == TenantedDeploymentMode.Untenanted))
            {
                untenantedProject.TenantedDeploymentMode = TenantedDeploymentMode.TenantedOrUntenanted;
                Repository.Projects.Modify(untenantedProject);
            }
        }


        ReferenceCollection GetEnvironmentsForProject(ProjectResource project, IDictionary<string, LifecycleResource> lifecycleIndex)
        {
            return new ReferenceCollection(lifecycleIndex[project.LifecycleId]
                .Phases.SelectMany(phase => phase.AutomaticDeploymentTargets.Concat(phase.OptionalDeploymentTargets))
                .TakeRandomSubset(EnvironmentsPerProjectLink.Get()));
        }
    }


    public static class Extendsions
    {
        public static IEnumerable<T> TakeRandomSubset<T>(this IEnumerable<T> elements, int countToTake)
        {
            var internalList = elements.ToList();
            countToTake = Math.Min(countToTake, internalList.Count);

            var selected = new List<T>();
            for (var i = 0; i < countToTake; ++i)
            {
                var next = Program.Rnd.Next(0, internalList.Count - selected.Count);
                selected.Add(internalList[next]);
                internalList[next] = internalList[internalList.Count - selected.Count];
            }
            return selected;
        }
    }
}
