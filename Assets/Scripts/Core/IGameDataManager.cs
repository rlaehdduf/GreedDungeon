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
        
        PlayerDataSO GetPlayerData();
        
        MonsterDataSO GetMonsterData(int id);
        SkillDataSO GetSkillData(int id);
        EquipmentDataSO GetEquipmentData(int id);
        ConsumableDataSO GetConsumableData(int id);
        RarityDataSO GetRarityData(int id);
        StatusEffectDataSO GetStatusEffectData(int id);
        MonsterSkillDataSO GetMonsterSkillData(int id);
        
        IReadOnlyList<MonsterDataSO> GetAllMonsterData();
        IReadOnlyList<SkillDataSO> GetAllSkillData();
        
        MonsterDataSO GetRandomMonsterData();
        MonsterDataSO GetBossMonsterData();
        IReadOnlyList<EquipmentDataSO> GetAllEquipmentData();
        IReadOnlyList<ConsumableDataSO> GetAllConsumableData();
        IReadOnlyList<RarityDataSO> GetAllRarityData();
        IReadOnlyList<MonsterSkillDataSO> GetAllMonsterSkillData();
        
        Task<GameObject> LoadMonsterPrefabAsync(string address);
        Task<GameObject> LoadSkillIconAsync(string address);
    }
}