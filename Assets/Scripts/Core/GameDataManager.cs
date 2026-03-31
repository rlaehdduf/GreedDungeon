using System.Collections.Generic;
using System.Threading.Tasks;
using GreedDungeon.ScriptableObjects;
using Pathfinder.Core.DI;
using UnityEngine;

namespace GreedDungeon.Core
{
    public class GameDataManager : IGameDataManager
    {
        public bool IsInitialized { get; private set; }

        [Inject] private IAssetLoader _assetLoader;

        private readonly Dictionary<int, MonsterDataSO> _monsters = new Dictionary<int, MonsterDataSO>();
        private readonly Dictionary<int, SkillDataSO> _skills = new Dictionary<int, SkillDataSO>();
        private readonly Dictionary<int, EquipmentDataSO> _equipments = new Dictionary<int, EquipmentDataSO>();
        private readonly Dictionary<int, ConsumableDataSO> _consumables = new Dictionary<int, ConsumableDataSO>();
        private readonly Dictionary<int, RarityDataSO> _rarities = new Dictionary<int, RarityDataSO>();
        private readonly Dictionary<int, StatusEffectDataSO> _statusEffects = new Dictionary<int, StatusEffectDataSO>();
        private readonly Dictionary<int, MonsterSkillDataSO> _monsterSkills = new Dictionary<int, MonsterSkillDataSO>();

        private PlayerDataSO _playerData;

        private readonly List<MonsterDataSO> _monsterList = new List<MonsterDataSO>();
        private readonly List<SkillDataSO> _skillList = new List<SkillDataSO>();
        private readonly List<EquipmentDataSO> _equipmentList = new List<EquipmentDataSO>();
        private readonly List<ConsumableDataSO> _consumableList = new List<ConsumableDataSO>();
        private readonly List<RarityDataSO> _rarityList = new List<RarityDataSO>();
        private readonly List<MonsterSkillDataSO> _monsterSkillList = new List<MonsterSkillDataSO>();

        public async Task InitializeAsync()
        {
            if (IsInitialized) return;

            await LoadAllDataAsync();
            IsInitialized = true;
            Debug.Log($"[GameDataManager] Initialized - Monsters: {_monsters.Count}, Skills: {_skills.Count}");
        }

        private async Task LoadAllDataAsync()
        {
            await Task.WhenAll(
                LoadDataAsync("MonsterData", _monsters, _monsterList),
                LoadDataAsync("SkillData", _skills, _skillList),
                LoadDataAsync("EquipmentData", _equipments, _equipmentList),
                LoadDataAsync("ConsumableData", _consumables, _consumableList),
                LoadDataAsync("RarityData", _rarities, _rarityList),
                LoadDataAsync("StatusEffectData", _statusEffects),
                LoadDataAsync("MonsterSkillData", _monsterSkills, _monsterSkillList),
                LoadPlayerDataAsync()
            );
        }

        private async Task LoadPlayerDataAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<PlayerDataSO>("PlayerData");
            foreach (var asset in assets)
            {
                _playerData = asset;
                break;
            }
        }

        private async Task LoadDataAsync<T>(string label, Dictionary<int, T> dict, List<T> list = null) where T : ScriptableObject
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<T>(label);
            foreach (var asset in assets)
            {
                if (asset is IData data)
                {
                    dict[data.ID] = asset;
                    list?.Add(asset);
                }
            }
        }

        public MonsterDataSO GetMonsterData(int id) => _monsters.TryGetValue(id, out var data) ? data : null;
        public SkillDataSO GetSkillData(int id) => _skills.TryGetValue(id, out var data) ? data : null;
        public EquipmentDataSO GetEquipmentData(int id) => _equipments.TryGetValue(id, out var data) ? data : null;
        public ConsumableDataSO GetConsumableData(int id) => _consumables.TryGetValue(id, out var data) ? data : null;
        public RarityDataSO GetRarityData(int id) => _rarities.TryGetValue(id, out var data) ? data : null;
        public StatusEffectDataSO GetStatusEffectData(int id) => _statusEffects.TryGetValue(id, out var data) ? data : null;
        public MonsterSkillDataSO GetMonsterSkillData(int id) => _monsterSkills.TryGetValue(id, out var data) ? data : null;
        public PlayerDataSO GetPlayerData() => _playerData;

        public IReadOnlyList<MonsterDataSO> GetAllMonsterData() => _monsterList;
        public IReadOnlyList<SkillDataSO> GetAllSkillData() => _skillList;
        
        public MonsterDataSO GetRandomMonsterData()
        {
            if (_monsterList == null || _monsterList.Count == 0) return null;
            var normalMonsters = _monsterList.FindAll(m => !m.IsBoss);
            if (normalMonsters == null || normalMonsters.Count == 0) return _monsterList[0];
            return normalMonsters[UnityEngine.Random.Range(0, normalMonsters.Count)];
        }
        
        public MonsterDataSO GetBossMonsterData()
        {
            if (_monsterList == null) return null;
            var bosses = _monsterList.FindAll(m => m.IsBoss);
            if (bosses == null || bosses.Count == 0) return null;
            return bosses[UnityEngine.Random.Range(0, bosses.Count)];
        }
        public IReadOnlyList<EquipmentDataSO> GetAllEquipmentData() => _equipmentList;
        public IReadOnlyList<ConsumableDataSO> GetAllConsumableData() => _consumableList;
        public IReadOnlyList<RarityDataSO> GetAllRarityData() => _rarityList;
        public IReadOnlyList<MonsterSkillDataSO> GetAllMonsterSkillData() => _monsterSkillList;

        public async Task<GameObject> LoadMonsterPrefabAsync(string address)
        {
            return await _assetLoader.LoadAssetAsync<GameObject>(address);
        }

        public async Task<GameObject> LoadSkillIconAsync(string address)
        {
            return await _assetLoader.LoadAssetAsync<GameObject>(address);
        }
    }

    public interface IData
    {
        int ID { get; }
    }
}