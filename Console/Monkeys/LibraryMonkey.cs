using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Octopus.Client;
using Octopus.Client.Model;

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
            var currentCount = Repository.Feeds.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateFeed(x);
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
            var currentCount = Repository.LibraryVariableSets.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateScriptModule(x);
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
            var test = Repository.LibraryVariableSets.FindAll();
            var currentCount = Repository.LibraryVariableSets.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
            {
                var offset = x * 100;
                var libraryVariableSet = CreateLibraryVariableSet(x);
                var variableSet = Repository.VariableSets.Get(libraryVariableSet.VariableSetId);
                for (var y = variableSet.Variables.Count(); y < numberOfVariablesPerRecord; y++)
                {
                    variableSet.AddOrUpdateVariableValue("VariableKey" + (offset + y).ToString("000"), "Hello sailor!");
                    variableSet = Repository.VariableSets.Modify(variableSet);
                }
            }
        }

        public void CreateCertificates(int numberOfRecords)
        {
            var count = Repository.Certificates.FindAll().Count();
            for (var i = count; i < numberOfRecords; i++)
            {
                CreateCertificate(i);
            }   
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

        //public void UpdateLibraryVariableSetVariableValues(int numberOfRecords, int numberOfVariablesPerRecord)
        //{
        //    for (var x = 0; x < numberOfRecords; x++)
        //    {
        //        var offset = x * 100;
        //        var libraryVariableSet = Repository.LibraryVariableSets.FindByName("LibraryVariableSet-" + x.ToString("000"));
        //        var variableSet = Repository.VariableSets.Get(libraryVariableSet.VariableSetId);
        //        for (var y = 0; y < numberOfVariablesPerRecord; y++)
        //        {
        //            variableSet.AddOrUpdateVariableValue("VariableKey" + (offset + y).ToString("000"), "Hello sailor 6!");
        //            variableSet = Repository.VariableSets.Modify(variableSet);
        //        }
        //    }
        //}

        #endregion

        #region TenantTagSets

        public void CreateTenantTagSets(int numberOfRecords)
        {
            var currentCount = Repository.TagSets.FindAll().Count();
            for (var x = currentCount; x < numberOfRecords; x++)
                CreateTenantTagSet(x);
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
