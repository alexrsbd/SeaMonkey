using Serilog;

namespace SeaMonkey
{
    public class TroopRunner : ITroopRunner
    {
        public void Run(ITroopRequest request)
        {
            Log.Information("Starting monkey business...");

            var commandFactory = new MonkeyCommandFactory(request);

            foreach (var breed in request.GetMonkeyBreeds())
            {
                var command = commandFactory.GetCommand(breed);
                Log.Information($"Running {command.GetType().Name}...");
                command.Run();
            }
        }
    }
}