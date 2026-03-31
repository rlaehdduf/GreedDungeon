using System.Collections;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Dungeon.UI;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.UI.Battle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GreedDungeon.Dungeon
{
    public class DungeonController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TreasurePopupUI _treasurePopupUI;
        [SerializeField] private ShopUI _shopUI;
        [SerializeField] private BackgroundScroller _backgroundScroller;
        [SerializeField] private MonsterDisplay _monsterDisplay;
        
        [Header("References")]
        [SerializeField] private Combat.BattleController _battleController;
        
        [Header("Settings")]
        [SerializeField] private int _minGoldReward = 10;
        [SerializeField] private int _maxGoldReward = 50;
        [SerializeField] private bool _testMode = false;
        
        private DungeonProgress _progress;
        private EncounterSystem _encounterSystem;
        private IGameDataManager _gameDataManager;
        private Player _player;
        private Monster _currentMonster;
        private DungeonState _currentState;
        private EncounterType? _forceNextEncounter = null;
        
        public DungeonState CurrentState => _currentState;
        
        private void Start()
        {
            StartCoroutine(WaitForDI());
        }
        
        private IEnumerator WaitForDI()
        {
            while (!Services.IsInitialized)
            {
                yield return null;
            }
            
            _gameDataManager = Services.Get<IGameDataManager>();
            
            if (_battleController != null)
            {
                _battleController.OnBattleStarted += HandleBattleStarted;
            }
            
            if (_treasurePopupUI != null)
            {
                _treasurePopupUI.OnConfirmed += HandleTreasureConfirmed;
            }
            
            if (_shopUI != null)
            {
                _shopUI.OnLeave += HandleShopLeave;
            }
        }
        
        private void Update()
        {
            if (_testMode && Keyboard.current.oKey.wasPressedThisFrame)
            {
                _forceNextEncounter = EncounterType.Shop;
            }

            if (_testMode && Keyboard.current.pKey.wasPressedThisFrame && _currentMonster != null)
            {
                _currentMonster.TakeDamage(100);
                if (_currentMonster.IsDead)
                {
                    OnMonsterDeath();
                }
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _progress = new DungeonProgress(player);
            _encounterSystem = new EncounterSystem(_progress);
            
            if (_treasurePopupUI != null)
            {
                _treasurePopupUI.Initialize(player);
            }
            
            if (_shopUI != null)
            {
                _shopUI.Initialize(player);
            }
        }
        
        private void HandleBattleStarted(Monster monster)
        {
            _currentMonster = monster;
            _currentState = DungeonState.Battle;
        }
        
        public void OnMonsterDeath()
        {
            GiveRandomConsumable();
            
            if (_monsterDisplay != null)
            {
                _monsterDisplay.Clear();
            }
            StartCoroutine(HandleMonsterDeathCoroutine());
        }
        
        private void GiveRandomConsumable()
        {
            if (_gameDataManager == null || _player == null) return;
            
            var consumables = _gameDataManager.GetAllConsumableData();
            if (consumables != null && consumables.Count > 0)
            {
                var consumable = consumables[UnityEngine.Random.Range(0, consumables.Count)];
                _player.TryAddItem(new InventoryItem(consumable, 1));
            }
        }
        
        private IEnumerator HandleMonsterDeathCoroutine()
        {
            yield return new WaitForSeconds(1f);
            
            EncounterType encounter;
            if (_forceNextEncounter.HasValue)
            {
                encounter = _forceNextEncounter.Value;
                _forceNextEncounter = null;
            }
            else
            {
                encounter = _encounterSystem.GetNextEncounter(afterBattle: true);
            }
            
            switch (encounter)
            {
                case EncounterType.Treasure:
                    ShowTreasure();
                    break;
                case EncounterType.Shop:
                    ShowShop();
                    break;
                case EncounterType.Battle:
                default:
                    MoveToNextBattle();
                    break;
            }
        }
        
        private void ShowTreasure()
        {
            _currentState = DungeonState.Treasure;
            
            int gold = UnityEngine.Random.Range(_minGoldReward, _maxGoldReward + 1);
            _player.AddGold(gold);
            
            InventoryItem item = GenerateRandomItem();
            if (item != null)
            {
                _player.TryAddItem(item);
            }
            
            if (_treasurePopupUI != null)
            {
                _treasurePopupUI.ShowTreasure(gold, item);
            }
            else
            {
                MoveToNextBattle();
            }
        }
        
        private void HandleTreasureConfirmed()
        {
            MoveToNextBattle();
        }
        
        private void ShowShop()
        {
            _currentState = DungeonState.Shop;
            
            if (_shopUI != null)
            {
                _shopUI.ShowShop();
            }
            else
            {
                MoveToNextBattle();
            }
        }
        
        private void HandleShopLeave()
        {
            MoveToNextBattle();
        }
        
        private void MoveToNextBattle()
        {
            _currentState = DungeonState.Moving;
            
            if (_backgroundScroller != null)
            {
                _backgroundScroller.ScrollToNextRoom(StartNextBattle);
            }
            else
            {
                StartNextBattle();
            }
        }
        
        private void StartNextBattle()
        {
            if (_backgroundScroller != null)
            {
                _backgroundScroller.ResetAlpha();
            }
            
            MonsterDataSO monsterData;
            
            bool isBoss = false;
            if (_progress != null && _encounterSystem != null)
            {
                isBoss = _encounterSystem.ShouldSpawnBoss();
            }
            
            if (isBoss)
            {
                monsterData = _gameDataManager.GetBossMonsterData();
                if (monsterData != null)
                {
                    _currentState = DungeonState.Boss;
                }
                else
                {
                    monsterData = _gameDataManager.GetRandomMonsterData();
                    _currentState = DungeonState.Battle;
                }
            }
            else
            {
                monsterData = _gameDataManager.GetRandomMonsterData();
                _currentState = DungeonState.Battle;
            }
            
            if (monsterData == null)
            {
                monsterData = _gameDataManager.GetMonsterData(1);
            }
            
            if (monsterData != null && _battleController != null)
            {
                _battleController.StartNewBattle(new Monster(monsterData));
            }
        }
        
        private InventoryItem GenerateRandomItem()
        {
            if (_gameDataManager == null) return null;
            
            if (UnityEngine.Random.value < 0.3f)
            {
                var consumables = _gameDataManager.GetAllConsumableData();
                if (consumables != null && consumables.Count > 0)
                {
                    var consumable = consumables[UnityEngine.Random.Range(0, consumables.Count)];
                    return new InventoryItem(consumable, 1);
                }
            }
            else
            {
                var equipments = _gameDataManager.GetAllEquipmentData();
                if (equipments != null && equipments.Count > 0)
                {
                    var equipment = equipments[UnityEngine.Random.Range(0, equipments.Count)];
                    return new InventoryItem(equipment, null, null);
                }
            }
            
            return null;
        }
        
        private void OnDestroy()
        {
            if (_battleController != null)
            {
                _battleController.OnBattleStarted -= HandleBattleStarted;
            }
            
            if (_treasurePopupUI != null)
            {
                _treasurePopupUI.OnConfirmed -= HandleTreasureConfirmed;
            }
            
            if (_shopUI != null)
            {
                _shopUI.OnLeave -= HandleShopLeave;
            }
        }
    }
}