# Sea Monkey

Sea Monkey is a simple but awesome tool to help populate an Octopus instance with a lot of test data.

**WARNING: Only use this tool against test/development Octopus instances where you need to simulate load**

To populate a bunch of test data, perform the following:

- Open the solution and build the solution.
- Launch your favourite console app/command prompt and navigate to the project's `bin/debug/netcoreapp...` folder.
- Execute `.\SeaMonkey.exe --server=http://localhost:8065 --apiKey=API-1234` at a Windows command prompt, replacing the server and apiKey with your own values, and your choice of monkey runners:
  - --runSetupMonkey
  - --runTenantMonkey
  - --runDeployMonkey
  - --runConfigurationMonkey
  - --runInfrastructureMonkey
  - --runLibraryMonkey
  - --runVariablesMonkey

E.g. Running the library monkey:
```
PS Z:\development\SeaMonkey\Console\bin\Debug\netcoreapp3.1> .\SeaMonkey.exe --server=http://localhost:8065 --apiKey=API-1234 --runLibraryMonkey
```

E.g. Deploying packages on an empty database:
```
PS Z:\development\SeaMonkey\Console\bin\Debug\netcoreapp3.1> .\SeaMonkey.exe --server=http://localhost:8065 --apiKey=API-1234 --runSetupMonkey --runInfrastructureMonkey --runDeployMonkey
```

NOTE: You'll likely need to update the `Octopus.Client` NuGet package dependency.
