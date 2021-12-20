using System;
using SeaMonkey.Exceptions;
using Serilog;

namespace SeaMonkey
{
    public interface IStartup
    {
        void Run(string[] args);
    }

    public class Startup : IStartup
    {
        private readonly ITroopRequestParser _requestParser;
        private readonly ITroopRunner _troopRunner;

        public Startup(ITroopRequestParser requestParser, ITroopRunner troopRunner)
        {
            _requestParser = requestParser;
            _troopRunner = troopRunner;
        }

        public void Run(string[] args)
        {
            try
            {
                var request = _requestParser.Parse(args);
                _troopRunner.Run(request);
            }
            catch (InvalidTroopRequestException ex)
            {
                Log.Error(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OOPS");
            }
            finally
            {
                Console.WriteLine("Done. Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}