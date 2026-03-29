using UnityEngine;

namespace Clicker
{
    [CreateAssetMenu(fileName = "ClickerGameConfig", menuName = "Clicker/GameConfig")]
    public class ClickerGameConfig : ScriptableObject
    {
        [field: SerializeField] public float RewardPerClick { get; private set; }
        [field: SerializeField] public float EnergyCostPerClick { get; private set; }
        [field: SerializeField] public float EnergyRefillDelaySeconds { get; private set; }
        [field: SerializeField] public float EnergyRefillAmount { get; private set; }
        [field: SerializeField] public float AutoClickDelaySeconds { get; private set; }
        [field: SerializeField] public float PlayerStartEnergy { get; private set; }
        [field: SerializeField] public float PlayerMaxEnergy { get; private set; }
    }
}