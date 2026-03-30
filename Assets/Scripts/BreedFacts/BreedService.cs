using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Core;

namespace BreedFacts
{
    public class BreedService
    {
        private readonly WebRequestService _web;
        private readonly JsonParser _parser;
        private readonly Breeds.BreedRequestConfig _config;

        private List<BreedData> _cached = new List<BreedData>();

        public IReadOnlyList<BreedData> Cached => _cached;

        public BreedService(WebRequestService web, JsonParser parser, Breeds.BreedRequestConfig config)
        {
            _web = web;
            _parser = parser;
            _config = config;
        }

        public async UniTask<List<BreedData>> LoadBreeds()
        {
            var json = await _web.GetAsync(_config.PublicApiBreedsList);
            var response = _parser.Parse<BreedListResponse>(json);

            _cached.Clear();

            if (response?.data == null)
                return _cached;

            for (int i = 0; i < response.data.Length && i < 10; i++)
            {
                var item = response.data[i];

                _cached.Add(new BreedData
                {
                    Id = item.id,
                    Name = item.attributes.name,
                    Description = item.attributes.description
                });
            }

            return _cached;
        }

        public async UniTask<BreedData> LoadBreed(string id, CancellationToken token)
        {
            var url = $"{_config.PublicApiGetBreedData}/{id}";
            var json = await _web.GetAsync(url, token);

            var response = _parser.Parse<BreedItem>(json);

            if (response == null)
                return null;

            return new BreedData
            {
                Id = response.id,
                Name = response.attributes.name,
                Description = response.attributes.description
            };
        }

        public void Cancel() => _web.CancelCurrent();
    }
}