using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BreedFacts
{
    public class BreedButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _id;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private GameObject _loader;
        [SerializeField] private Button _button;
    }
}
