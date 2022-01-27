using Serilog;
using SerilogTimings;

namespace SeaMonkey
{
    public class TroopRunner : ITroopRunner
    {
        public void Run(ITroopRequest request)
        {
            Log.Information("Starting monkey business...");

            var commandFactory = new MonkeyCommandFactory(request);

            using var troopOp = Operation.Time("Running this troop");
            foreach (var breed in request.GetMonkeyBreeds())
            {
                var command = commandFactory.GetCommand(breed);

                Log.Information("Running {Command}...", command.GetType().Name);
                using var breedOp = Operation.Time("Running {Command}", command.GetType().Name);

                command.Run();
            }
        }
    }
}