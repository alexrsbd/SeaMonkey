using System.Collections.Generic;
using System.Linq;

namespace SeaMonkey.ProbabilitySets
{
    public class SubsetProbability<T>
    {
        private readonly LinearProbability _linearProbability;
        private readonly IReadOnlyList<T> _set;

        public SubsetProbability(int atLeast, int upTo, IEnumerable<T> set)
        {
            _linearProbability = new LinearProbability(atLeast, upTo);
            _set = set.ToList();
        }

        public IEnumerable<T> Get()
        {
            var numberOfElements = _linearProbability.Get();
            return _set.OrderBy(x => Program.Rnd.Next()).Take(numberOfElements);
        }
    }
}