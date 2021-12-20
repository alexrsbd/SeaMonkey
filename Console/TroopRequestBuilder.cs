using System.Collections.Generic;

namespace SeaMonkey
{
    public class TroopRequestBuilder
    {
        private string _serverAddress;
        private string _apiKey;
        private readonly SortedSet<MonkeyBreed> _breeds = new SortedSet<MonkeyBreed>();

        public TroopRequestBuilder WithServerAddress(string address)
        {
            _serverAddress = address;
            return this;
        }

        public TroopRequestBuilder WithApiKey(string apiKey)
        {
            _apiKey = apiKey;
            return this;
        }

        public TroopRequestBuilder WithMonkeyBreed(MonkeyBreed breed)
        {
            _breeds.Add(breed);
            return this;
        }

        public TroopRequest Build()
        {
            return new TroopRequest(_serverAddress, _apiKey, _breeds);
        }
    }
}