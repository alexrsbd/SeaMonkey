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
	.ForAll(i =>
	{
		try
		{
			var endpoint = new OctopusServerEndpoint($"http://{i.PublicIp}:81/");
			var repository = new OctopusRepository(endpoint);
			repository.Users.SignIn("Admin", "ThePassword");
			repository.Client.Put("/api/licenses/licenses-current", new LicenceResource());
		}
		catch(Exception ex)
		{
			Console.WriteLine($"{i.Machine} {i.PublicIp} {ex.Message}");
		}
	}
	);
	
}

class Instance
{
	public string Machine { get; set; }
	public string PublicIp { get; set; }
	public string PrivateIp { get; set; }
}

class LicenceResource : Resource
{
	public LicenceResource()
	{
		LicenseText = @"????";
	}
	public string LicenseText { get; set; }
}