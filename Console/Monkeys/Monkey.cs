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
        public IntProbability VariablesPerProject { get; set; } = new DiscretProbability(10, 20, 100);


        protected Monkey(OctopusRepository repository)
        {
            Repository = repository;
        }

        protected IReadOnlyList<MachineResource> GetMachines()
        {
            var machines = Repository.Machines.FindAll();
            foreach(var machine in machines.Where(m => !m.Roles.Contains("InstallStuff")))
            {
                machine.Roles.Add("InstallStuff");
                Repository.Machines.Modify(machine);
            }
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

        protected void SetVariables(ProjectResource project)
        {
            var numberOfVariables = VariablesPerProject.Get();
            for (var p = 1; p <= numberOfVariables; p++)
            {
                var variableSet = Repository.VariableSets.Get(project.VariableSetId);
                variableSet.Variables = Enumerable.Range(1, numberOfVariables)
                    .Select(n => new VariableResource()
                    {
                        Name = "Variable " + n.ToString("000"),
                        Value = "This is the variable value for Variable " + n.ToString("000")
                    })
                    .ToArray();

                Repository.VariableSets.Modify(variableSet);
            }
        }
    }
}