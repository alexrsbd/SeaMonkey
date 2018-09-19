<Query Kind="Statements">
  <NuGetReference Version="4.36.3-enh-workers58" Prerelease="true">Octopus.Client</NuGetReference>
  <Namespace>Octopus.Client</Namespace>
  <Namespace>Octopus.Client.Model</Namespace>
  <Namespace>Octopus.Client.Model.Endpoints</Namespace>
  <Namespace>Octopus.Client.Serialization</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

// This will print out all packages that are referenced by releases currently in the selected environment

var endpoint = new OctopusServerEndpoint("", "");
var repository = new OctopusRepository(endpoint);

var env = repository.Environments.FindByName("Test");

var rnd = new Random();

var tags = repository.TagSets.GetAll();
var tenants = repository.Tenants.GetAll();
CreateTags();

CreateTenant();

void CreateMachines()
{
	
Enumerable.Range(1, 1000)
	.AsParallel()
	.WithDegreeOfParallelism(5)
	.ForAll(e =>
	{
		repository.Machines.Create(new MachineResource()
		{
			Name = $"Cloud {e:0000}",
			Endpoint = new CloudRegionEndpointResource(),
			EnvironmentIds = new ReferenceCollection(env.Id),
			Roles = new ReferenceCollection("Cloud"),
			TenantTags = new ReferenceCollection(GetTags()),
			TenantIds = new ReferenceCollection(GetTenants())
		});
	});


}

void CreateTenant()
{

	Enumerable.Range(1, 500)
		.AsParallel()
		.WithDegreeOfParallelism(5)
		.ForAll(e =>
		{
			repository.Tenants.Create(new TenantResource
			{
				Name = "Tenants " + e.ToString("000"),
				TenantTags = new ReferenceCollection(GetTags()),

			});
		});
}

void CreateTags()
{
	Enumerable.Range(1, 100)
	.AsParallel()
		.WithDegreeOfParallelism(5)
		.ForAll(e =>
		{
			repository.TagSets.Create(new TagSetResource()
			{
				Name = "Set " + e.ToString("000"),
				Tags = Enumerable.Range(1, 10)
						.Select(t => new TagResource()
						{
							Name = $"Tag {e:000} {t:00}",
							Color = $"#{e - 1:00}{e - 1:00}{t * 10 - 1:00}"
						})
						.ToList()
			});
		});

}

IEnumerable<string> GetTenants()
{
	foreach (var tenant in tenants)
	{
		if (rnd.Next(30) == 0)
		{
			yield return tenant.Id;
		}
	}
}

List<string> GetTags()
{
	var common = new List<string> {
		tags[0].Tags[rnd.Next(0, tags[0].Tags.Count)].CanonicalTagName,
		tags[1].Tags[rnd.Next(0, tags[1].Tags.Count)].CanonicalTagName,
	};


	foreach (var tag in tags.Skip(2))
	{
		if (rnd.Next(30) == 0)
		{
			common.Add(tag.Tags[rnd.Next(0, tags[1].Tags.Count)].CanonicalTagName);
		}
	}
	return common;
}