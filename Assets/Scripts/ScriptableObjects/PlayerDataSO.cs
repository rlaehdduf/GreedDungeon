using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "GreedDungeon/Data/Player")]
    public class PlayerDataSO : ScriptableObject, IData
    {
        [Header("기본 스탯")]
        public int BaseMaxHP = 100;
        public int BaseMaxMP = 50;
        public int BaseAttack = 10;
        public int BaseDefense = 5;
        public int BaseSpeed = 10;
        public float BaseCriticalRate = 5f;

        [Header("레벨업 보너스")]
        [Tooltip("레벨당 기본 스탯에 더해지는 비율 (0.1 = 10%)")]
        public float StatBonusPerLevel = 0.1f;

        [Header("시작 자원")]
        public int StartingGold = 0;

        [Header("인벤토리")]
        public int InventorySize = 21;

        int IData.ID => 1;
    }
}