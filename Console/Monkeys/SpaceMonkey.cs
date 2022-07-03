using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using Serilog;

namespace SeaMonkey.Monkeys
{
    public class SpaceMonkey : Monkey
    {
        public SpaceMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void Create(int numberOfRecords)
        {
            Log.Information("Creating {n} spaces", numberOfRecords);

            Enumerable.Range(1, numberOfRecords)
                .AsParallel()
                .ForAll(i => {
                    var name = "Space " + i.ToString("000");
                    var resource = Repository.Spaces.FindByName(name);
                    if (resource is not null)
                    {
                        Log.Information("Space {name} already exists. Skipping.", name);
                        return;
                    }

                    Repository.Spaces.Create(new SpaceResource()
                    {
                        Name = name,
                        SpaceManagersTeams = new ReferenceCollection("teams-administrators") // Give all admins access to new spaces.
                    });
                    Log.Information("Created space {name}", name);
                });
        }
    }
}
