using System.Collections.Generic;
using System.Threading.Tasks;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Core
{
    public interface IGameDataManager
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        
        MonsterDataSO GetMonsterData(int id);
        SkillDataSO GetSkillData(int id);
        EquipmentDataSO GetEquipmentData(int id);
        ConsumableDataSO GetConsumableData(int id);
        RarityDataSO GetRarityData(int id);
        StatusEffectDataSO GetStatusEffectData(int id);
        
        IReadOnlyList<MonsterDataSO> GetAllMonsterData();
        IReadOnlyList<SkillDataSO> GetAllSkillData();
        IReadOnlyList<EquipmentDataSO> GetAllEquipmentData();
        IReadOnlyList<ConsumableDataSO> GetAllConsumableData();
        
        Task<GameObject> LoadMonsterPrefabAsync(string address);
        Task<GameObject> LoadSkillIconAsync(string address);
    }
}