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
			repository.Users.SignIn("Admin", "password");
			var tasks = repository.Tasks.FindAll(pathParameters: new { name = "Deploy" });
			return new Result
			{
				Machine = i.Machine,
				IP = i.PublicIp,
				Queued = tasks.Count(t => t.StartTime == null),
				Executing = tasks.Count(t => t.State == TaskState.Executing),
				Completed = tasks.Count(t => t.CompletedTime != null),
				Failed = tasks.Count(t => t.State == TaskState.Failed)
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{i.Machine} {i.PublicIp} {ex.Message}");
			return new Result
			{
				Machine = i.Machine,
				IP = i.PublicIp
			};
		}
	}
	)
	.Dump();
	
}

class Result
{
	public string Machine { get; set; }
	public string IP { get; set; }
	public int? Queued { get; set; }
	public int? Executing { get; set; }
	public int? Completed { get; set; }
	public int? Failed { get; set; }
}

class Instance
{
	public string Machine { get; set; }
	public string PublicIp { get; set; }
	public string PrivateIp { get; set; }
}

class TaskHeaderResource  {
	public TotalCounts TotalCounts { get;set;}
}

class TotalCounts
{
	public int Canceled { get; set; }
	public int Cancelling { get; set; }
	public int Executing { get; set; }
	public int Failed { get; set; }
	public int Queued { get; set; }
	public int Success { get; set; }
	public int TimedOut { get; set; }
	public int Interrupted { get; set; }
}