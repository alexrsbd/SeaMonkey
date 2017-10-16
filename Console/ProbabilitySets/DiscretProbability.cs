namespace SeaMonkey.ProbabilitySets
{
    public class DiscretProbability : IntProbability
    {
        private readonly int[] _values;

        public DiscretProbability(params int[] values)
        {
            _values = values;
        }

        public override int Get()
        {
            return _values[Program.Rnd.Next(0, _values.Length)];
        }
    }
}