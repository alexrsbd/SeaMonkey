<Query Kind="Statements">
  <NuGetReference>Octopus.Client</NuGetReference>
  <Namespace>Octopus.Client</Namespace>
  <Namespace>Octopus.Client.Model</Namespace>
  <Namespace>Octopus.Client.Model.Endpoints</Namespace>
  <Namespace>Octopus.Client.Serialization</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

var endpoint = new OctopusServerEndpoint("https://cattleclass.tentaclearmy.com:8085", "API-DYJ11WUMQATHENVDH811QFBP3O");
var repository = new OctopusRepository(endpoint);

var machines = repository.Machines.FindAll().OrderBy(m => new string(m.Name.Reverse().ToArray()));

for (int x = 1; x < 500; x++)
{
	foreach (var machine in machines.Skip(x % 150).Take(1))
	{
		x.Dump();
		var n = x;
		var id = machine.Name.Substring(machine.Name.LastIndexOf("-") + 1);
		var instance = "TestInstance" + id;
		var port = 10000 + n;
		var server = "http://52.189.198.113/" + n.ToString("000");

		//var command = $@"register-with --instance={instance} --server {server} --name {machine.Name} --server-comms-port={port}  --username Admin --password !CattleClass! --environment Test --comms-style TentacleActive --role Tentacle"
		var command = $@"register-worker --instance={instance} --server {server} --name {machine.Name} --server-comms-port={port}  --username Admin --password !CattleClass! --workerpool ""Default Worker Pool"" --comms-style TentacleActive";
		

		repository.Tasks.Create(new TaskResource
		{
			Name = "AdHocScript",
			Description = "Direct Machine " + machine.Name + " to " + server,
			Arguments = new Dictionary<string, object>
	{
		{"MachineIds", new[] { machine.Id }},
		{"ScriptBody", $@". 'c:\program files\octopus deploy\tentacle\Tentacle' {command}
						  . 'c:\program files\octopus deploy\tentacle\Tentacle' service --instance={instance} --stop --start"}
	},
		});
	}
}