using System;
using UnityEngine;

namespace Forecast
{
    public class WeatherJSONParser
    {
        [Serializable]
        private class WeatherJSONData
        {
            public Properties properties;
        }

        [Serializable]
        private class Properties
        {
            public Period[] periods;
        }

        [Serializable]
        private class Period
        {
            public string name;
            public int temperature;
            public string temperatureUnit;
            public string icon;
        }

        public WeatherData ParseWeatherData(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                var response = JsonUtility.FromJson<WeatherJSONData>(json);
                
                if (response?.properties?.periods == null || response.properties.periods.Length == 0)
                {
                    return null;
                }

                var firstPeriod = response.properties.periods[0];

                var weatherData = new WeatherData
                {
                    Name = firstPeriod.name,
                    Temperature = firstPeriod.temperature,
                    TemperatureUnit = firstPeriod.temperatureUnit,
                    Icon = firstPeriod.icon
                };

                return weatherData;
            }
            catch (Exception)
            {
                return null;
            }
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