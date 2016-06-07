namespace SeaMonkey.ProbabilitySets
{
    public class LinearProbability : IntProbability
    {
        private readonly int _min;
        private readonly int _max;

        public LinearProbability(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public override int Get()
        {
            return Program.Rnd.Next(_min, _max + 1);
        }
    }
}