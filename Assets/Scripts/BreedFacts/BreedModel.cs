using System;

namespace BreedFacts
{
    [Serializable]
    public class BreedListResponse
    {
        public BreedItem[] data;
    }

    [Serializable]
    public class BreedItem
    {
        public string id;
        public BreedAttributes attributes;
    }

    [Serializable]
    public class BreedAttributes
    {
        public string name;
        public string description;
    }

    public class BreedData
    {
        public string Id;
        public string Name;
        public string Description;
    }
}