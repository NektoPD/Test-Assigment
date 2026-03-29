using UnityEngine;

namespace Breeds
{
    [CreateAssetMenu(fileName = "BreedsConfig", menuName = "Breeds/Config")]   
    public class BreedRequestConfig : ScriptableObject
    {
        public string PublicApi;
    }
}