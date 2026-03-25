# 던전 방 전환 시스템 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 몬스터 처치 후 조우 시스템(전투/보물/상점)을 통해 다음 방으로 전환

**Architecture:** DungeonController가 BattleController의 몬스터 사망 이벤트를 구독하고, EncounterSystem으로 다음 조우를 결정한 후 해당 UI를 표시하고 다음 전투를 시작

**Tech Stack:** Unity 2D, C#, Services DI 패턴, UIView 기반 UI

---

## File Structure

**Create:**
- `Assets/Scripts/Dungeon/DungeonState.cs` - 던전 상태 enum
- `Assets/Scripts/Dungeon/EncounterType.cs` - 조우 타입 enum
- `Assets/Scripts/Dungeon/DungeonProgress.cs` - 진행도 추적 (처치 수, 보스 확률)
- `Assets/Scripts/Dungeon/EncounterSystem.cs` - 조우 확률 계산
- `Assets/Scripts/Dungeon/BackgroundScroller.cs` - 배경 스크롤 애니메이션
- `Assets/Scripts/Dungeon/DungeonController.cs` - 전체 흐름 제어
- `Assets/Scripts/Dungeon/UI/TreasurePopupUI.cs` - 보물 획득 팝업
- `Assets/Scripts/Dungeon/UI/ShopUI.cs` - 상점 UI

**Modify:**
- `Assets/Scripts/Combat/BattleController.cs:435-437` - HandleMonsterDeath에서 DungeonController 호출

---

### Task 1: 기본 데이터 구조

**Files:**
- Create: `Assets/Scripts/Dungeon/DungeonState.cs`
- Create: `Assets/Scripts/Dungeon/EncounterType.cs`
- Create: `Assets/Scripts/Dungeon/DungeonProgress.cs`

- [ ] **Step 1: DungeonState enum 생성**

```csharp
namespace GreedDungeon.Dungeon
{
    public enum DungeonState
    {
        Battle,
        Moving,
        Treasure,
        Shop,
        Boss
    }
}
```

- [ ] **Step 2: EncounterType enum 생성**

```csharp
namespace GreedDungeon.Dungeon
{
    public enum EncounterType
    {
        Battle,
        Treasure,
        Shop
    }
}
```

- [ ] **Step 3: DungeonProgress 클래스 생성**

```csharp
using GreedDungeon.Character;

namespace GreedDungeon.Dungeon
{
    public class DungeonProgress
    {
        private readonly Player _player;
        
        public int KillCount => _player.KillCount;
        public bool IsBossDefeated { get; private set; }
        
        public DungeonProgress(Player player)
        {
            _player = player;
        }
        
        public int GetBossProbability()
        {
            if (KillCount < 10) return 0;
            return (KillCount - 9) * 10;
        }
        
        public bool RollForBoss()
        {
            int probability = GetBossProbability();
            if (probability <= 0) return false;
            if (probability >= 100) return true;
            return UnityEngine.Random.Range(0, 100) < probability;
        }
        
        public void MarkBossDefeated()
        {
            IsBossDefeated = true;
        }
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Dungeon/
git commit -m "feat: add dungeon state and progress classes"
```

---

### Task 2: 조우 시스템

**Files:**
- Create: `Assets/Scripts/Dungeon/EncounterSystem.cs`

- [ ] **Step 1: EncounterSystem 클래스 생성**

```csharp
using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class EncounterSystem
    {
        private const int BATTLE_CHANCE = 60;
        private const int TREASURE_CHANCE = 25;
        private const int SHOP_CHANCE = 15;
        
        private readonly DungeonProgress _progress;
        
        public EncounterSystem(DungeonProgress progress)
        {
            _progress = progress;
        }
        
        public EncounterType GetNextEncounter()
        {
            int roll = Random.Range(0, 100);
            
            if (roll < BATTLE_CHANCE)
            {
                return EncounterType.Battle;
            }
            else if (roll < BATTLE_CHANCE + TREASURE_CHANCE)
            {
                return EncounterType.Treasure;
            }
            else
            {
                return EncounterType.Shop;
            }
        }
        
        public bool ShouldSpawnBoss()
        {
            return _progress.RollForBoss();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/Dungeon/EncounterSystem.cs
git commit -m "feat: add encounter system with probability"
```

---

### Task 3: 배경 스크롤러

**Files:**
- Create: `Assets/Scripts/Dungeon/BackgroundScroller.cs`

- [ ] **Step 1: BackgroundScroller 클래스 생성**

```csharp
using System;
using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _scrollDuration = 0.5f;
        [SerializeField] private float _scaleAmount = 0.1f;
        
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        
        public event Action OnScrollComplete;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }
        
        public void ScrollToNextRoom(Action onComplete = null)
        {
            StartCoroutine(ScrollCoroutine(onComplete));
        }
        
        private System.Collections.IEnumerator ScrollCoroutine(Action onComplete)
        {
            float elapsed = 0f;
            Vector3 targetScale = _originalScale;
            targetScale.y += _scaleAmount;
            
            while (elapsed < _scrollDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _scrollDuration;
                
                float scaleOffset = Mathf.Sin(t * Mathf.PI) * _scaleAmount;
                Vector3 newScale = _originalScale;
                newScale.y += scaleOffset;
                _rectTransform.localScale = newScale;
                
                yield return null;
            }
            
            _rectTransform.localScale = _originalScale;
            onComplete?.Invoke();
            OnScrollComplete?.Invoke();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/Dungeon/BackgroundScroller.cs
git commit -m "feat: add background scroller animation"
```

---

### Task 4: 보물 팝업 UI

**Files:**
- Create: `Assets/Scripts/Dungeon/UI/TreasurePopupUI.cs`

- [ ] **Step 1: TreasurePopupUI 클래스 생성**

```csharp
using System;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Dungeon.UI
{
    public class TreasurePopupUI : UIView
    {
        [Header("UI Elements")]
        [SerializeField] private Text _goldText;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private InventorySlotUI _itemSlotPrefab;
        [SerializeField] private Button _confirmButton;
        
        private Player _player;
        private IGameDataManager _gameDataManager;
        
        public event Action OnConfirmed;
        
        private void Start()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(HandleConfirm);
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _gameDataManager = Services.Get<IGameDataManager>();
        }
        
        public void ShowTreasure(int gold, InventoryItem item)
        {
            if (_goldText != null)
            {
                _goldText.text = $"+{gold}G";
            }
            
            ClearItemSlots();
            
            if (item != null && _itemContainer != null && _itemSlotPrefab != null)
            {
                var slot = Instantiate(_itemSlotPrefab, _itemContainer);
                slot.SetItem(item);
            }
            
            base.Show();
        }
        
        private void ClearItemSlots()
        {
            if (_itemContainer == null) return;
            
            for (int i = _itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_itemContainer.GetChild(i).gameObject);
            }
        }
        
        private void HandleConfirm()
        {
            Hide();
            OnConfirmed?.Invoke();
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/Dungeon/UI/TreasurePopupUI.cs
git commit -m "feat: add treasure popup UI"
```

---

### Task 5: 상점 UI

**Files:**
- Create: `Assets/Scripts/Dungeon/UI/ShopUI.cs`

- [ ] **Step 1: ShopUI 클래스 생성**

```csharp
using System;
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Items;
using GreedDungeon.ScriptableObjects;
using GreedDungeon.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.Dungeon.UI
{
    public class ShopUI : UIView
    {
        [Header("Shop Items")]
        [SerializeField] private Transform _shopSlotsContainer;
        [SerializeField] private InventorySlotUI _slotPrefab;
        
        [Header("Player Inventory (for selling)")]
        [SerializeField] private Transform _playerInventoryContainer;
        [SerializeField] private InventorySlotUI _playerSlotPrefab;
        
        [Header("UI Elements")]
        [SerializeField] private ItemTooltipUI _tooltipUI;
        [SerializeField] private Button _leaveButton;
        [SerializeField] private Text _playerGoldText;
        
        private const int SHOP_SLOT_COUNT = 3;
        private const int ITEM_BUY_PRICE_MULTIPLIER = 2;
        private const int ITEM_SELL_PRICE_DIVISOR = 2;
        
        private Player _player;
        private IGameDataManager _gameDataManager;
        private readonly List<InventoryItem> _shopItems = new List<InventoryItem>();
        
        public event Action OnLeave;
        
        private void Start()
        {
            if (_leaveButton != null)
            {
                _leaveButton.onClick.AddListener(HandleLeave);
            }
        }
        
        public void Initialize(Player player)
        {
            _player = player;
            _gameDataManager = Services.Get<IGameDataManager>();
            
            if (_player != null)
            {
                _player.OnInventoryChanged += UpdatePlayerInventoryDisplay;
            }
        }
        
        public void ShowShop()
        {
            GenerateShopItems();
            UpdateShopDisplay();
            UpdatePlayerInventoryDisplay();
            UpdatePlayerGold();
            base.Show();
        }
        
        private void GenerateShopItems()
        {
            _shopItems.Clear();
            
            if (_gameDataManager == null) return;
            
            var allEquipment = _gameDataManager.GetAllEquipmentData();
            var allConsumables = _gameDataManager.GetAllConsumableData();
            
            for (int i = 0; i < SHOP_SLOT_COUNT; i++)
            {
                if (Random.value < 0.7f && allEquipment != null && allEquipment.Count > 0)
                {
                    var equipment = allEquipment[Random.Range(0, allEquipment.Count)];
                    var item = new InventoryItem(equipment, null, null);
                    _shopItems.Add(item);
                }
                else if (allConsumables != null && allConsumables.Count > 0)
                {
                    var consumable = allConsumables[Random.Range(0, allConsumables.Count)];
                    var item = new InventoryItem(consumable, 1);
                    _shopItems.Add(item);
                }
            }
        }
        
        private void UpdateShopDisplay()
        {
            if (_shopSlotsContainer == null || _slotPrefab == null) return;
            
            foreach (Transform child in _shopSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _shopItems.Count; i++)
            {
                var item = _shopItems[i];
                var slot = Instantiate(_slotPrefab, _shopSlotsContainer);
                slot.SetItem(item);
                
                int index = i;
                var button = slot.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => TryBuyItem(index));
                }
            }
        }
        
        private void UpdatePlayerInventoryDisplay()
        {
            if (_playerInventoryContainer == null || _playerSlotPrefab == null || _player == null) return;
            
            foreach (Transform child in _playerInventoryContainer)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _player.InventorySize; i++)
            {
                var item = _player.GetItemAt(i);
                if (item == null) continue;
                
                var slot = Instantiate(_playerSlotPrefab, _playerInventoryContainer);
                slot.SetItem(item);
                
                int index = i;
                var button = slot.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => TrySellItem(index));
                }
            }
        }
        
        private void UpdatePlayerGold()
        {
            if (_playerGoldText != null && _player != null)
            {
                _playerGoldText.text = $"{_player.Gold}G";
            }
        }
        
        private void TryBuyItem(int index)
        {
            if (_player == null || index < 0 || index >= _shopItems.Count) return;
            
            var item = _shopItems[index];
            int price = GetBuyPrice(item);
            
            if (_player.Gold < price)
            {
                Debug.Log("골드가 부족합니다.");
                return;
            }
            
            if (_player.IsInventoryFull())
            {
                Debug.Log("인벤토리가 가득 찼습니다.");
                return;
            }
            
            _player.SpendGold(price);
            _player.TryAddItem(item.Clone());
            _shopItems.RemoveAt(index);
            
            UpdateShopDisplay();
            UpdatePlayerGold();
        }
        
        public void TrySellItem(int inventoryIndex)
        {
            if (_player == null) return;
            
            var item = _player.GetItemAt(inventoryIndex);
            if (item == null) return;
            
            int sellPrice = GetSellPrice(item);
            
            _player.RemoveItemAt(inventoryIndex);
            _player.AddGold(sellPrice);
            
            UpdatePlayerInventoryDisplay();
            UpdatePlayerGold();
        }
        
        private int GetBuyPrice(InventoryItem item)
        {
            if (item == null) return 0;
            
            if (item.Type == ItemType.Equipment)
            {
                return (item.Equipment?.BasePrice ?? 50) * ITEM_BUY_PRICE_MULTIPLIER;
            }
            else
            {
                return (item.Consumable?.BasePrice ?? 20) * ITEM_BUY_PRICE_MULTIPLIER;
            }
        }
        
        private int GetSellPrice(InventoryItem item)
        {
            if (item == null) return 0;
            
            if (item.Type == ItemType.Equipment)
            {
                return (item.Equipment?.BasePrice ?? 25) / ITEM_SELL_PRICE_DIVISOR;
            }
            else
            {
                return ((item.Consumable?.BasePrice ?? 10) / ITEM_SELL_PRICE_DIVISOR) * item.Quantity;
            }
        }
        
        private void HandleLeave()
        {
            Hide();
            OnLeave?.Invoke();
        }
        
        public override void Hide()
        {
            base.Hide();
            
            if (_player != null)
            {
                _player.OnInventoryChanged -= UpdatePlayerInventoryDisplay;
            }
        }
    }
}
```

- [ ] **Step 2: InventoryItem.Clone 메서드 추가**

`Assets/Scripts/Items/InventoryItem.cs`에 추가:

```csharp
public InventoryItem Clone()
{
    if (Type == ItemType.Consumable)
    {
        return new InventoryItem(Consumable, Quantity);
    }
    else
    {
        return new InventoryItem(Equipment, Rarity, Skill);
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Dungeon/UI/ShopUI.cs Assets/Scripts/Items/InventoryItem.cs
git commit -m "feat: add shop UI with buy and sell functionality"
```

---

### Task 6: 던전 컨트롤러

**Files:**
- Create: `Assets/Scripts/Dungeon/DungeonController.cs`
- Modify: `Assets/Scripts/Combat/BattleController.cs`

- [ ] **Step 1: DungeonController 클래스 생성**

```csharp
using System.Collections;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.Dungeon.UI;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class DungeonController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TreasurePopupUI _treasurePopupUI;
        [SerializeField] private ShopUI _shopUI;
        [SerializeField] private BackgroundScroller _backgroundScroller;
        
        [Header("References")]
        [SerializeField] private Combat.BattleController _battleController;
        
        [Header("Settings")]
        [SerializeField] private int _minGoldReward = 10;
        [SerializeField] private int _maxGoldReward = 50;
        
        private DungeonProgress _progress;
        private EncounterSystem _encounterSystem;
        private IGameDataManager _gameDataManager;
        private Player _player;
        private Monster _currentMonster;
        private DungeonState _currentState;
        
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
            StartCoroutine(HandleMonsterDeathCoroutine());
        }
        
        private IEnumerator HandleMonsterDeathCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            
            var encounter = _encounterSystem.GetNextEncounter();
            
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
            
            int gold = Random.Range(_minGoldReward, _maxGoldReward + 1);
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
            var encounter = _encounterSystem.GetNextEncounter();
            bool isBoss = _encounterSystem.ShouldSpawnBoss();
            
            MonsterDataSO monsterData;
            
            if (isBoss)
            {
                monsterData = _gameDataManager.GetBossMonsterData();
                _currentState = DungeonState.Boss;
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
            
            if (monsterData != null)
            {
                var newMonster = new Monster(monsterData);
                _battleController.StartNewBattle(newMonster);
            }
        }
        
        private InventoryItem GenerateRandomItem()
        {
            if (_gameDataManager == null) return null;
            
            if (Random.value < 0.3f)
            {
                var consumables = _gameDataManager.GetAllConsumableData();
                if (consumables != null && consumables.Count > 0)
                {
                    var consumable = consumables[Random.Range(0, consumables.Count)];
                    return new InventoryItem(consumable, 1);
                }
            }
            else
            {
                var equipments = _gameDataManager.GetAllEquipmentData();
                if (equipments != null && equipments.Count > 0)
                {
                    var equipment = equipments[Random.Range(0, equipments.Count)];
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
```

- [ ] **Step 2: BattleController 수정 - StartNewBattle 메서드 추가**

`Assets/Scripts/Combat/BattleController.cs`에 StartNewBattle 메서드 추가:

```csharp
public void StartNewBattle(Monster monster)
{
    if (_gameDataManager == null) return;
    
    _currentMonster = monster;
    
    if (_testPlayer == null)
    {
        _testPlayer = new Player();
        AddTestItemsToInventory();
    }
    
    if (_battleUI != null)
    {
        _battleUI.SetupBattle(_testPlayer, _currentMonster);
    }
    
    _battleManager.StartBattle(_testPlayer, _currentMonster);
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Dungeon/DungeonController.cs Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: add dungeon controller with room transition"
```

---

### Task 7: IGameDataManager 확장

**Files:**
- Modify: `Assets/Scripts/Core/IGameDataManager.cs`
- Modify: `Assets/Scripts/Core/GameDataManager.cs`

- [ ] **Step 1: IGameDataManager 인터페이스에 메서드 추가**

```csharp
MonsterDataSO GetRandomMonsterData();
MonsterDataSO GetBossMonsterData();
```

- [ ] **Step 2: GameDataManager에 구현 추가**

```csharp
public MonsterDataSO GetRandomMonsterData()
{
    if (_monsterDataList == null || _monsterDataList.Count == 0) return null;
    return _monsterDataList[UnityEngine.Random.Range(0, _monsterDataList.Count)];
}

public MonsterDataSO GetBossMonsterData()
{
    var bosses = _monsterDataList?.FindAll(m => m.IsBoss);
    if (bosses == null || bosses.Count == 0) return null;
    return bosses[UnityEngine.Random.Range(0, bosses.Count)];
}
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Core/IGameDataManager.cs Assets/Scripts/Core/GameDataManager.cs
git commit -m "feat: add random and boss monster data methods"
```

---

### Task 8: BattleController와 DungeonController 연결

**Files:**
- Modify: `Assets/Scripts/Combat/BattleController.cs`

- [ ] **Step 1: DungeonController 참조 추가**

BattleController에 DungeonController 참조 추가:

```csharp
[SerializeField] private Dungeon.DungeonController _dungeonController;
```

- [ ] **Step 2: WaitForDI 코루틴에서 DungeonController 초기화**

WaitForDI 코루틴 끝에 추가:

```csharp
if (_dungeonController != null)
{
    _dungeonController.Initialize(_testPlayer);
}
```

- [ ] **Step 3: HandleMonsterDeath에서 DungeonController 호출 추가**

기존 HandleMonsterDeath 메서드에 DungeonController 호출 추가 (기존 로직 유지):

```csharp
private void HandleMonsterDeath()
{
    if (_dungeonController != null)
    {
        _dungeonController.OnMonsterDeath();
    }
}
```

참고: HandleMonsterDeath는 현재 비어있으므로(435-437행), 이 호출을 추가하는 것이 기존 로직을 덮어쓰지 않습니다.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: connect BattleController with DungeonController"
```

---

## Unity Editor Setup (Manual)

After implementing the code, the following Unity Editor steps are required:

1. Create UI for TreasurePopupUI:
   - Panel with CanvasGroup
   - Text for gold amount
   - Container for item slots
   - Confirm button

2. Create UI for ShopUI:
   - Panel with CanvasGroup
   - Shop slots container (3 slots)
   - Player inventory container (for selling)
   - Tooltip UI reference (show item name + buy/sell price)
   - Leave button
   - Player gold text

3. Add BackgroundScroller to background image

4. Add DungeonController to Battle scene:
   - Connect UI references
   - Connect BattleController reference

5. Update BattleController:
   - Connect DungeonController reference

6. ItemTooltipUI 수정 (가격 표시용):
   - 가격 텍스트 추가
   - ShopUI에서 호출 시 가격 표시 로직 추가