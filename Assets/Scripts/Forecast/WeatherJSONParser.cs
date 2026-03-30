using System;
using Core;

namespace Forecast
{
    [Serializable]
    public class WeatherJSONData
    {
        public WeatherProperties properties;
    }

    [Serializable]
    public class WeatherProperties
    {
        public WeatherPeriod[] periods;
    }

    [Serializable]
    public class WeatherPeriod
    {
        public string name;
        public int temperature;
        public string temperatureUnit;
        public string icon;
    }

    public class WeatherJSONParser
    {
        private readonly JsonParser _parser;

        public WeatherJSONParser(JsonParser parser)
        {
            _parser = parser;
        }

        public WeatherData ParseWeatherData(string json)
        {
            var response = _parser.Parse<WeatherJSONData>(json);

            if (response?.properties?.periods == null || response.properties.periods.Length == 0)
                return null;

            var p = response.properties.periods[0];

            return new WeatherData
            {
                Name = p.name,
                Temperature = p.temperature,
                TemperatureUnit = p.temperatureUnit,
                Icon = p.icon
            };
        }
    }

    public class WeatherData
    {
        public string Name;
        public int Temperature;
        public string TemperatureUnit;
        public string Icon;
    }
}