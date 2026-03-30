using UnityEngine;

namespace Forecast
{
    [CreateAssetMenu(fileName = "WeatherForecastConfig", menuName = "WeatherForecast/Config")]
    public class WeatherForecastConfig : ScriptableObject
    {
        public string PublicApi;
        public float RequestIntervalSeconds = 5f;
    }
}