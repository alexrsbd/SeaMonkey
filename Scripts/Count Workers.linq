<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.JScript.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.Install.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Management.dll</Reference>
  <NuGetReference>Octopus.Client</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Management.Instrumentation</Namespace>
  <Namespace>Octopus.Client</Namespace>
  <Namespace>Octopus.Client.Serialization</Namespace>
  <Namespace>Octopus.Client.Model</Namespace>
</Query>

void Main()
{
	var instances = JsonSerialization.DeserializeObject<Instance[]>(File.ReadAllText(@"C:\Users\rober\Downloads\services.json"));
	
	instances
	.AsParallel()
	.WithDegreeOfParallelism(10)
	.Select(i =>
	{
		try
		{
			var endpoint = new OctopusServerEndpoint($"http://{i.PublicIp}:81/");
			var repository = new OctopusRepository(endpoint);
			repository.Users.SignIn("Admin", "Password");
			var workers = repository.Workers.FindAll();
			return (i.Machine, i.PrivateIp, workers.Count, workers.Count(w => w.HealthStatus != MachineModelHealthStatus.Healthy || w.HealthStatus != MachineModelHealthStatus.HasWarnings));
		}
		catch(Exception ex)
		{
			Console.WriteLine($"{i.Machine} {i.PublicIp} {ex.Message}");
			return (i.Machine, i.PublicIp, -1, -1);
		}
	}
	)
	.Dump();
	
}

class Instance
{
	public string Machine { get; set; }
	public string PublicIp { get; set; }
	public string PrivateIp { get; set; }
}