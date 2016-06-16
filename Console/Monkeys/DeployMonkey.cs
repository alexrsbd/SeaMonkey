using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using SeaMonkey.ProbabilitySets;
using Serilog;
using Serilog.Events;

namespace SeaMonkey.Monkeys
{
    public class DeployMonkey : Monkey
    {
        private readonly Random _rnd = new Random(235346798);

        public DeployMonkey(OctopusRepository repository) : base(repository)
        {

        }


        public class ProjectInfo
        {
            public ProjectResource Project { get; set; }
            public ReleaseResource LatestRelease { get; set; }
            public ReferenceCollection EnvironmentIds { get; set; }
            public IList<ChannelResource> Channels { get; set; }
            public DeploymentProcessResource DeploymentProcess { get; set; }
            public IReadOnlyList<TenantResource> Tenants { get; set; }
        }

        public BooleanProbability ChanceOfANewRelease { get; set; } = new BooleanProbability(0.25);
        public BooleanProbability ChanceOfAProcessChangeOnNewRelease { get; set; } = new BooleanProbability(0.75);

        public void RunForAllProjects(int maxNumberOfDeployments = int.MaxValue)
        {
            Run(GetProjectInfos(Repository.Projects.FindAll()), maxNumberOfDeployments);
        }

        public void RunForProject(string name, int maxNumberOfDeployments = int.MaxValue)
        {
            Run(GetProjectInfos(new[] { Repository.Projects.FindByName(name)}), maxNumberOfDeployments);
        }

        public void RunForGroup(string name, int maxNumberOfDeployments = int.MaxValue)
        {
            var group = Repository.ProjectGroups.FindByName(name);
            Run(GetProjectInfos(Repository.ProjectGroups.GetProjects(group)), maxNumberOfDeployments);
        }

        private void Run(IReadOnlyList<ProjectInfo> projectInfos, int maxNumberOfDeployments = int.MaxValue)
        {
            var projectEnvsQ = from p in projectInfos
                               from e in p.EnvironmentIds
                               select new
                               {
                                   ProjectInfo = p,
                                   EnvironmentId = e,
                                   Tenant = (TenantResource) null
                               };
            var projectTenantEnvsQ = from p in projectInfos
                                     from t in p.Tenants
                                     from e in t.ProjectEnvironments[p.Project.Id]
                                     select new
                                     {
                                         ProjectInfo = p,
                                         EnvironmentId = e,
                                         Tenant = t
                                     };

            var projectEnvs = projectEnvsQ.Concat(projectTenantEnvsQ).ToArray();

            for (var cnt = 1; cnt <= maxNumberOfDeployments; cnt++)
            {
                var item = projectEnvs[_rnd.Next(0, projectEnvs.Length)];

                if (item.ProjectInfo.LatestRelease == null || ChanceOfANewRelease.Get())
                    CreateRelease(item.ProjectInfo);

                CreateDeployment(item.ProjectInfo, item.EnvironmentId, item.Tenant);


                Log.Write(cnt % 10 == 0 ? LogEventLevel.Information : LogEventLevel.Verbose, "{n} deployments", cnt);
            }
        }

        private ProjectInfo[] GetProjectInfos(IReadOnlyList<ProjectResource> projects)
        {
            var lifecycles = Repository.Lifecycles.FindAll().ToArray();
            
            var releases = from r in Repository.Releases.FindAll()
                           let x = new { r.ProjectId, Release = r, Version = SemanticVersion.Parse(r.Version) }
                           group x by x.ProjectId
                           into g
                           select new
                           {
                               ProjectId = g.Key,
                               LatestRelease = g.OrderByDescending(r => r.Version).First().Release
                           };

            var tenants = Repository.Tenants.FindAll();

            var q = from p in projects
                    join r in releases on p.Id equals r.ProjectId into rj
                    from r in rj.DefaultIfEmpty()
                    join l in lifecycles on p.LifecycleId equals l.Id
                    select new ProjectInfo
                    {
                        Project = p,
                        LatestRelease = r?.LatestRelease,
                        EnvironmentIds = l.Phases[0].OptionalDeploymentTargets,
                        Channels = Repository.Projects.GetChannels(p).Items,
                        DeploymentProcess = Repository.DeploymentProcesses.Get(p.DeploymentProcessId),
                        Tenants = tenants.Where(t => t.ProjectEnvironments.ContainsKey(p.Id)).ToArray()
                    };

            return q.ToArray();
        }


        private void CreateRelease(ProjectInfo projectInfo)
        {
            if (ChanceOfAProcessChangeOnNewRelease.Get())
                projectInfo.DeploymentProcess = UpdateDeploymentProcess(projectInfo.Project);

            var newVersion = projectInfo.LatestRelease == null ? "1.0.0.0" : SemanticVersion.Parse(projectInfo.LatestRelease.Version).Increment().ToString();
            var release = new ReleaseResource()
            {
                ChannelId = projectInfo.Channels[Program.Rnd.Next(0, projectInfo.Channels.Count)].Id,
                ProjectId = projectInfo.Project.Id,
                Version = newVersion,
                SelectedPackages = projectInfo.DeploymentProcess
                    .Steps
                    .SelectMany(s => s.Actions)
                    .Where(a => a.Properties.ContainsKey("Octopus.Action.Package.NuGetPackageId"))
                    .Select(a => new SelectedPackage(a.Name, "3.2.4"))
                    .ToList()
            };
            projectInfo.LatestRelease = Repository.Releases.Create(release);
        }

        public void CreateDeployment(ProjectInfo projectInfo, string environmentId, TenantResource tenant)
        {
            Repository.Deployments.Create(new DeploymentResource()
            {
                ProjectId = projectInfo.Project.Id,
                ReleaseId = projectInfo.LatestRelease.Id,
                TenantId = tenant?.Id,
                EnvironmentId = environmentId,
                ForcePackageRedeployment = true
            });
        }
    }

}