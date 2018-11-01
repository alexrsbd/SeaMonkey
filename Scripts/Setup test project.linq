<Query Kind="Program">
  <NuGetReference>Octopus.Client</NuGetReference>
  <Namespace>Octopus.Client</Namespace>
  <Namespace>Octopus.Client.Model</Namespace>
  <Namespace>Octopus.Client.Model.Endpoints</Namespace>
  <Namespace>Octopus.Client.Serialization</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>Octopus.Client.Model.DeploymentProcess</Namespace>
  <Namespace>Octopus.Client.Model.Triggers</Namespace>
  <Namespace>Octopus.Client.Model.Triggers.ScheduledTriggers</Namespace>
</Query>

void Main()
{
	var instances = JsonSerialization.DeserializeObject<Instance[]>(File.ReadAllText(@"C:\Users\rober\Downloads\services.json"));
	var n = 0;
	foreach (var instance in instances)
	{
		n++;
		$"{n} {instance.Machine}".Dump();
		var endpoint = new OctopusServerEndpoint($"http://{instance.PublicIp}:81/");
		var repository = new OctopusRepository(endpoint);
	repository.Users.SignIn("Admin", "ThePassword");


		var environment = repository.Environments.FindByName("Test");
		if (environment == null)
			environment = repository.Environments.Create(new EnvironmentResource { Name = "Test" });

		var lifecycle = repository.Lifecycles.FindAll().First();

		var project = repository.Projects.FindByName("Test");
		if (project == null)
			project = repository.Projects.Create(new ProjectResource
			{
				Name = "Test",
				ProjectGroupId = repository.ProjectGroups.FindAll().First().Id,
				LifecycleId = lifecycle.Id
			});

		var variables = repository.VariableSets.Get(project.VariableSetId);
		variables.Variables.Clear();
		variables.AddOrUpdateVariableValue("Octopus.Acquire.DeltaCompressionEnabled", "false");
		variables.AddOrUpdateVariableValue("Octopus.Deployment.ForcePackageDownload", "true");
		variables.AddOrUpdateVariableValue("OctopusBypassDeploymentMutex", "true");
		repository.VariableSets.Modify(variables);

		var process = repository.DeploymentProcesses.Get(project.DeploymentProcessId);
		process.ClearSteps();
		var step = process.AddOrUpdateStep("Transfer");
		step.TargetingRoles("Tentacle");
		var action = step.AddOrUpdateAction("Transfer");
		action.ActionType = "Octopus.TransferPackage";
		action.Packages.Add(new PackageReference()
		{
			Name = "",
			FeedId = "feedz-builtin",
			PackageId = "Lots",
			AcquisitionLocation = "Server"
		});
		action.Properties["Octopus.Action.Package.FeedId"] = "feeds-builtin";
		action.Properties["Octopus.Action.Package.PackageId"] = "Lots";
		action.Properties["Octopus.Action.Package.TransferPath"] = ".";

		step = process.AddOrUpdateStep("Script on server");
		step.TargetingRoles("Tentacle");
		step.AddOrUpdateScriptAction("Script on server", ScriptAction.InlineScript(ScriptSyntax.PowerShell, "dir\r\nstart-sleep 1"), ScriptTarget.Server);

		step = process.AddOrUpdateStep("Script on tentacle");
		step.TargetingRoles("Tentacle");
		step.AddOrUpdateScriptAction("Script on tentacle", ScriptAction.InlineScript(ScriptSyntax.PowerShell, @"for($x = 0; $x -lt 100; $x++)
{
    dir
    Start-sleep 1
}"), ScriptTarget.Target);

		var trigger = repository.Projects.GetTriggers(project).Items.FirstOrDefault(t => t.Name == "Deploy");
		if (trigger == null)
			trigger = new ProjectTriggerResource { ProjectId = project.Id, Name = "Deploy" };

		trigger.Action = new DeployNewReleaseActionResource
		{
			EnvironmentId = environment.Id
		};
		trigger.Filter = new DailyScheduledTriggerFilterResource
		{
			Timezone = "UTC",
			Interval = DailyScheduledTriggerInterval.OnceEveryMinute,
			RunType = ScheduledTriggerFilterRunType.Continuously,
			MinuteInterval = (short) ((n + 1) * 5)
		};

		var _ = trigger.Id == null ? repository.ProjectTriggers.Create(trigger) : repository.ProjectTriggers.Modify(trigger);

		repository.DeploymentProcesses.Modify(process);
	}

}

class Instance
{
	public string Machine { get; set; }
	public string PublicIp { get; set; }
	public string PrivateIp { get; set; }
}
