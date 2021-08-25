using System.Collections.Generic;
using System.Linq;
using Octopus.Client;
using Octopus.Client.Model;
using SeaMonkey.ProbabilitySets;

namespace SeaMonkey.Monkeys
{
    public abstract class Monkey
    {
        protected readonly OctopusRepository Repository;

        public IntProbability StepsPerProject { get; set; } = new FibonacciProbability(FibonacciProbability.Limit._5, FibonacciProbability.Limit._21);
        public IntProbability StepsPerRunbook { get; set; } = new FibonacciProbability(FibonacciProbability.Limit._1, FibonacciProbability.Limit._3);
        public IntProbability VariablesPerProject { get; set; } = new DiscretProbability(10, 20, 100);


        protected Monkey(OctopusRepository repository)
        {
            Repository = repository;
        }

        protected IReadOnlyList<MachineResource> GetMachines()
        {
            var machines = Repository.Machines.FindAll(pathParameters: new { take = int.MaxValue });
            machines
                .Where(m => !m.Roles.Contains("InstallStuff"))
                .AsParallel()
                .WithDegreeOfParallelism(30)
                .ForAll(machine =>
                    {
                        machine.Roles.Add("InstallStuff");
                        Repository.Machines.Modify(machine);
                    }
                );
            return machines;
        }

        protected DeploymentProcessResource UpdateDeploymentProcess(ProjectResource project)
        {
            var process = Repository.DeploymentProcesses.Get(project.DeploymentProcessId);
            process.Steps.Clear();
            var numberOfSteps = StepsPerProject.Get();
            for (var x = 1; x <= numberOfSteps; x++)
                process.Steps.Add(StepLibrary.Random(x));

            return Repository.DeploymentProcesses.Modify(process);
        }

        protected RunbookProcessResource UpdateRunbookProcess(RunbookResource runbook)
        {
            var process = Repository.RunbookProcesses.Get(runbook.RunbookProcessId);
            process.Steps.Clear();
            var numberOfSteps = StepsPerRunbook.Get();
            for (var x = 1; x <= numberOfSteps; x++)
                process.Steps.Add(StepLibrary.Random(x));

            return Repository.RunbookProcesses.Modify(process);
        }

        protected void SetVariables(ProjectResource project)
        {
            var numberOfVariables = VariablesPerProject.Get();
            var variableSet = Repository.VariableSets.Get(project.VariableSetId);

            Enumerable.Range(1, numberOfVariables)
                .ToList()
                .ForEach(n =>
                {
                    variableSet.AddOrUpdateVariableValue("Variable " + n.ToString("000"),
                        "This is the variable value for Variable " + n.ToString("000"));
                });

            // To help us reference a variable-based package reference for ARC testing.
            variableSet.AddOrUpdateVariableValue(StepLibrary.AcmePackageName, StepLibrary.AcmePackageName);

            Repository.VariableSets.Modify(variableSet);
        }
    }
}