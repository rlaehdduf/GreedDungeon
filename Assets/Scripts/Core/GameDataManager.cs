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

        private readonly List<MonsterDataSO> _monsterList = new List<MonsterDataSO>();
        private readonly List<SkillDataSO> _skillList = new List<SkillDataSO>();
        private readonly List<EquipmentDataSO> _equipmentList = new List<EquipmentDataSO>();
        private readonly List<ConsumableDataSO> _consumableList = new List<ConsumableDataSO>();

        public async Task InitializeAsync()
        {
            if (IsInitialized) return;

            await LoadAllDataAsync();
            IsInitialized = true;
            Debug.Log($"[GameDataManager] 초기화 완료 - 몬스터: {_monsters.Count}, 스킬: {_skills.Count}");
        }

        private async Task LoadAllDataAsync()
        {
            await Task.WhenAll(
                LoadMonstersAsync(),
                LoadSkillsAsync(),
                LoadEquipmentsAsync(),
                LoadConsumablesAsync(),
                LoadRaritiesAsync(),
                LoadStatusEffectsAsync()
            );
        }

        private async Task LoadMonstersAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<MonsterDataSO>("MonsterData");
            foreach (var asset in assets)
            {
                _monsters[asset.ID] = asset;
                _monsterList.Add(asset);
            }
        }

        private async Task LoadSkillsAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<SkillDataSO>("SkillData");
            foreach (var asset in assets)
            {
                _skills[asset.ID] = asset;
                _skillList.Add(asset);
            }
        }

        private async Task LoadEquipmentsAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<EquipmentDataSO>("EquipmentData");
            foreach (var asset in assets)
            {
                _equipments[asset.ID] = asset;
                _equipmentList.Add(asset);
            }
        }

        private async Task LoadConsumablesAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<ConsumableDataSO>("ConsumableData");
            foreach (var asset in assets)
            {
                _consumables[asset.ID] = asset;
                _consumableList.Add(asset);
            }
        }

        private async Task LoadRaritiesAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<RarityDataSO>("RarityData");
            foreach (var asset in assets)
            {
                _rarities[asset.ID] = asset;
            }
        }

        private async Task LoadStatusEffectsAsync()
        {
            var assets = await _assetLoader.LoadAllAssetsByLabelAsync<StatusEffectDataSO>("StatusEffectData");
            foreach (var asset in assets)
            {
                _statusEffects[asset.ID] = asset;
            }
        }

        public MonsterDataSO GetMonsterData(int id) => _monsters.TryGetValue(id, out var data) ? data : null;
        public SkillDataSO GetSkillData(int id) => _skills.TryGetValue(id, out var data) ? data : null;
        public EquipmentDataSO GetEquipmentData(int id) => _equipments.TryGetValue(id, out var data) ? data : null;
        public ConsumableDataSO GetConsumableData(int id) => _consumables.TryGetValue(id, out var data) ? data : null;
        public RarityDataSO GetRarityData(int id) => _rarities.TryGetValue(id, out var data) ? data : null;
        public StatusEffectDataSO GetStatusEffectData(int id) => _statusEffects.TryGetValue(id, out var data) ? data : null;

        public IReadOnlyList<MonsterDataSO> GetAllMonsterData() => _monsterList;
        public IReadOnlyList<SkillDataSO> GetAllSkillData() => _skillList;
        public IReadOnlyList<EquipmentDataSO> GetAllEquipmentData() => _equipmentList;
        public IReadOnlyList<ConsumableDataSO> GetAllConsumableData() => _consumableList;

        public async Task<GameObject> LoadMonsterPrefabAsync(string address)
        {
            return await _assetLoader.LoadAssetAsync<GameObject>(address);
        }

        public async Task<GameObject> LoadSkillIconAsync(string address)
        {
            return await _assetLoader.LoadAssetAsync<GameObject>(address);
        }
    }
}