namespace SeaMonkey.ProbabilitySets
{
    public class BooleanProbability
    {
        private readonly double _trueProbability;

        public BooleanProbability(double trueProbability)
        {
            _trueProbability = trueProbability;
        }

        public bool Get()
        {
            return Program.Rnd.NextDouble() <= _trueProbability;
        }
    }
}