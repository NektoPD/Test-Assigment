using System;
using UnityEngine;

namespace Core
{
    public class JsonParser
    {
        public T Parse<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
