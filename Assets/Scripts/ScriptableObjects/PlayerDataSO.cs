using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "GreedDungeon/Data/Player")]
    public class PlayerDataSO : ScriptableObject, IData
    {
        [Header("Base Stats")]
        public int BaseMaxHP = 100;
        public int BaseMaxMP = 50;
        public int BaseAttack = 10;
        public int BaseDefense = 5;
        public int BaseSpeed = 10;
        public float BaseCriticalRate = 5f;

        [Header("Level Up Bonus")]
        [Tooltip("Ratio added to base stats per level (0.1 = 10%)")]
        public float StatBonusPerLevel = 0.1f;

        [Header("Starting Resources")]
        public int StartingGold = 0;

        [Header("Inventory")]
        public int InventorySize = 21;

        int IData.ID => 1;
    }
}