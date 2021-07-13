using System;
using System.IO;
using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using Serilog;

namespace SeaMonkey.Monkeys
{
    public class LibraryMonkey : Monkey
    {
        public LibraryMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void CreateRecords(int numberOfFeeds,
            int numberOfScriptModules,
            int numberOfLibraryVariableSets,
            int numberOfLibraryVariableVariables,
            int numberOfTenantTagSets,
            int numberOfCertificates)
        {
            CreateFeeds(numberOfFeeds);
            CreateScriptModules(numberOfScriptModules);
            CreateLibraryVariableSets(numberOfLibraryVariableSets, numberOfLibraryVariableVariables);
            CreateTenantTagSets(numberOfTenantTagSets);
            CreateCertificates(numberOfCertificates);
        }

        #region Feeds

        public void CreateFeeds(int numberOfRecords)
        {
            Log.Information("Creating {n} feeds", numberOfRecords);
            var currentCount = Repository.Feeds.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateFeed(i));
        }

        private FeedResource CreateFeed(int prefix)
        {
            return
                Repository.Feeds.Create(new NuGetFeedResource()
                {
                    Name = "Feed-" + prefix.ToString("000"),
                    FeedUri = "https://api.nuget.org/v3/index.json",
                });
        }

        #endregion

        #region ScriptModule

        public void CreateScriptModules(int numberOfRecords)
        {
            Log.Information("Creating {n} script modules", numberOfRecords);
            var currentCount = Repository.LibraryVariableSets.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateScriptModule(i));
        }

        private LibraryVariableSetResource CreateScriptModule(int prefix)
        {
            return
                Repository.LibraryVariableSets.Create(new LibraryVariableSetResource()
                {
                    Name = "LibraryVariableSet-" + prefix.ToString("000"),
                    Description = "Let's get schwifty!",
                    ContentType = VariableSetContentType.ScriptModule,
                });
            //TODO: write an actual PowerShell script with the VariableSetId that comes back from this request.
        }

        #endregion

        #region LibraryVariableSets

        public void CreateLibraryVariableSets(int numberOfRecords, int numberOfVariablesPerRecord)
        {
            Log.Information("Creating {n} library variable sets", numberOfRecords);
            var currentCount = Repository.LibraryVariableSets.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i =>
                    {
                        var offset = i * 100;
                        var libraryVariableSet = CreateLibraryVariableSet(i);
                        var variableSet = Repository.VariableSets.Get(libraryVariableSet.VariableSetId);
                        for (var y = variableSet.Variables.Count(); y < numberOfVariablesPerRecord; y++)
                        {
                            variableSet.AddOrUpdateVariableValue("VariableKey" + (offset + y).ToString("000"), "Hello sailor!");
                            variableSet = Repository.VariableSets.Modify(variableSet);
                        }
                    }
                );
        }

        public void CreateCertificates(int numberOfRecords)
        {
            Log.Information("Creating {n} certificates", numberOfRecords);
            var currentCount = Repository.Certificates.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateCertificate(i));
        }

        private CertificateResource CreateCertificate(int prefix)
        {
            var data = Utils.ReadFileBinary(() => GetType().Assembly.GetManifestResourceStream("SeaMonkey.Monkeys.pickle-rick.pfx"));
            return Repository.Certificates.Create(new CertificateResource("Pickle-Cert" + prefix.ToString("000"), Convert.ToBase64String(data), "Morty"));

        }

        private LibraryVariableSetResource CreateLibraryVariableSet(int prefix)
        {
            return
                Repository.LibraryVariableSets.Create(new LibraryVariableSetResource()
                {
                    Name = "LibraryVariableSet-" + prefix.ToString("000"),
                    Description = "Rick: Uh-huh, yeah, that’s the difference between you and me, Morty. I never go back to the carpet store"
                });
        }

        #endregion

        #region TenantTagSets

        public void CreateTenantTagSets(int numberOfRecords)
        {
            Log.Information("Creating {n} tenant tags", numberOfRecords);
            var currentCount = Repository.TagSets.FindAll().Count();
            Enumerable.Range(currentCount, numberOfRecords)
                .AsParallel()
                .ForAll(i => CreateTenantTagSet(i));
        }

        private TagSetResource CreateTenantTagSet(int prefix)
        {
            return
                Repository.TagSets.Create(new TagSetResource()
                {
                    Name = "TenantTagSet-" + prefix.ToString("000"),
                    Description = "Listen, Morty, I hate to break it to you but what people call 'love' is just a chemical reaction that compels animals to breed. It hits hard, Morty, then it slowly fades, leaving you stranded in a failing marriage. I did it. Your parents are gonna do it. Break the cycle, Morty. Rise above. Focus on science"
                });
        }

        #endregion

    }

    public static class Utils
    {

        public static byte[] ReadFileBinary(Func<Stream> factory)
        {
            var buffer = new MemoryStream();

            using (var stream = factory())
            {
                stream.CopyTo(buffer);
                return buffer.ToArray();
            }
        }
    }
}
