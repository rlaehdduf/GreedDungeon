using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    [CreateAssetMenu(fileName = "StatusEffectData", menuName = "GreedDungeon/Data/StatusEffect")]
    public class StatusEffectDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        public int DamagePerTurn;
        [Range(0f, 1f)] public float DamageCurrentPercent;
        [Range(0f, 1f)] public float DamageMaxPercent;
        public int Duration;
        public bool SkipTurn;
        
        int IData.ID => ID;
    }
}