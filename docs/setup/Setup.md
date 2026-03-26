# Unity 수동 설정 가이드

## 필수 설정 (1회성)

1. **Build Settings**: `Title`, `Dungeon`, `Battle` 씬 추가
2. **Title 씬 DI**: GameRoot → RootContext + GameInstaller
3. **CSV 변환**: `Tools → CSV → Convert All`
4. **Addressables**: `Tools → Addressables → Set All Prefab Addresses` → Build

---

## Battle 씬 구조

```
Battle.unity
├── GameRoot (GameObject)
│   ├── RootContext (컴포넌트)
│   └── GameInstaller (컴포넌트)
├── Canvas (GameObject)
│   ├── EnemyInfomation, PlayerInfomation, LogUI
│   ├── AttackBtn, DefenseBtn, ItemBtn
│   ├── SkillSlot_1~3
│   └── BattleUI (컴포넌트)
├── Managers (GameObject)
│   ├── BattleController (컴포넌트)
│   ├── MonsterDisplay (컴포넌트)
│   └── DungeonController (컴포넌트)
├── Background (GameObject)
│   └── BackgroundScroller (컴포넌트)
└── EnemySpawnPoint (GameObject)
```

---

## 컴포넌트 연결

### BattleController (MonoBehaviour)
| 필드 | 타입 | 연결 대상 |
|------|------|-----------|
| _monsterDisplay | MonsterDisplay | MonsterDisplay 컴포넌트 |
| _battleUI | BattleUI | Canvas의 BattleUI 컴포넌트 |
| _dungeonController | DungeonController | DungeonController 컴포넌트 |

### BattleUI (MonoBehaviour)
| 필드 | 타입 | 연결 대상 |
|------|------|-----------|
| _playerStatus | PlayerStatusUI | PlayerInfomation의 컴포넌트 |
| _monsterStatus | MonsterStatusUI | EnemyInfomation의 컴포넌트 |
| _battleLog | BattleLogUI | LogUI의 컴포넌트 |
| _actionMenu | ActionMenuUI | 버튼 부모의 컴포넌트 |
| _skillSlotUI | SkillSlotUI | 스킬슬롯 부모의 컴포넌트 |
| _inventoryUI | InventoryUI | InventoryPanel의 컴포넌트 |

---

## 2026-03-25: 던전 방 전환 시스템

### 생성된 스크립트
```
Assets/Scripts/Dungeon/
├── DungeonState.cs          # enum (Battle, Moving, Treasure, Shop, Boss)
├── EncounterType.cs         # enum (Battle, Treasure, Shop)
├── DungeonProgress.cs       # class (KillCount, 보스확률)
├── EncounterSystem.cs       # class (조우 확률 계산)
├── BackgroundScroller.cs    # MonoBehaviour (배경 애니메이션)
├── DungeonController.cs     # MonoBehaviour (전체 흐름 제어)
└── UI/
    ├── TreasurePopupUI.cs   # UIView (보물 팝업)
    └── ShopUI.cs            # UIView (상점 UI)
```

---

### 1. TreasurePopup 프리팹

**위치:** `Assets/Prefabs/UI/TreasurePopup.prefab`

**계층 구조:**
```
TreasurePopup (GameObject) - 초기 비활성화
├── CanvasGroup (컴포넌트)
│   └── Alpha: 1, Interactable: true, Blocks Raycasts: true
├── Background (GameObject)
│   └── Image (컴포넌트) - Color: (0,0,0,200)
└── Content (GameObject)
    ├── Vertical Layout Group (컴포넌트)
    │   └── Spacing: 10, Padding: 20
    ├── TitleText (GameObject)
    │   └── Text (컴포넌트) - "보물 획득!", Font Size: 24, Alignment: Center
    ├── GoldText (GameObject)
    │   └── Text (컴포넌트) - "+50G", Font Size: 32, Color: Yellow
    ├── ItemContainer (GameObject)
    │   ├── Horizontal Layout Group (컴포넌트)
    │   │   └── Spacing: 5, Child Alignment: Middle Center
    │   └── Content Size Fitter (컴포넌트)
    │       └── Horizontal: Preferred, Vertical: Preferred
    └── ConfirmButton (GameObject)
        ├── Image (컴포넌트) - 버튼 배경
        └── Button (컴포넌트)
            └── 자식 Text: "확인"
```

**TreasurePopupUI 컴포넌트 설정:**
| 필드 | 타입 | 연결 대상 |
|------|------|-----------|
| _goldText | Text | GoldText의 Text 컴포넌트 |
| _itemContainer | Transform | ItemContainer의 Transform |
| _itemSlotPrefab | InventorySlotUI | `Assets/Prefabs/UI/InventorySlot.prefab` |
| _confirmButton | Button | ConfirmButton의 Button 컴포넌트 |

---

### 2. Shop 프리팹

**위치:** `Assets/Prefabs/UI/Shop.prefab`

**계층 구조:**
```
Shop (GameObject) - 초기 비활성화
├── CanvasGroup (컴포넌트)
│   └── Alpha: 1, Interactable: true, Blocks Raycasts: true
├── Background (GameObject)
│   └── Image (컴포넌트) - Color: (30,30,30,230)
└── Content (GameObject)
    ├── Horizontal Layout Group (컴포넌트)
    │   └── Spacing: 20, Padding: 20
    ├── LeftPanel (GameObject) - 상점 아이템
    │   ├── Vertical Layout Group (컴포넌트)
    │   ├── TitleText (GameObject)
    │   │   └── Text (컴포넌트) - "상점", Font Size: 20
    │   ├── ShopSlotsContainer (GameObject)
    │   │   ├── Vertical Layout Group (컴포넌트)
    │   │   │   └── Spacing: 10
    │   │   └── Content Size Fitter (컴포넌트)
    │   │       └── Vertical: Preferred
    │   └── PlayerGoldText (GameObject)
    │       └── Text (컴포넌트) - "1000G", Font Size: 18, Color: Yellow
    ├── RightPanel (GameObject) - 판매용 인벤토리
    │   ├── Vertical Layout Group (컴포넌트)
    │   ├── TitleText (GameObject)
    │   │   └── Text (컴포넌트) - "판매", Font Size: 20
    │   └── PlayerInventoryContainer (GameObject)
    │       ├── Grid Layout Group (컴포넌트)
    │       │   └── Cell Size: (80,80), Spacing: (10,10), Constraint: Fixed Column, Count: 4
    │       └── Content Size Fitter (컴포넌트)
    │           └── Vertical: Preferred
    └── LeaveButton (GameObject)
        ├── Image (컴포넌트)
        └── Button (컴포넌트)
            └── 자식 Text: "나가기"
```

**ShopUI 컴포넌트 설정:**
| 필드 | 타입 | 연결 대상 |
|------|------|-----------|
| _shopSlotsContainer | Transform | ShopSlotsContainer의 Transform |
| _slotPrefab | InventorySlotUI | `Assets/Prefabs/UI/InventorySlot.prefab` |
| _playerInventoryContainer | Transform | PlayerInventoryContainer의 Transform |
| _playerSlotPrefab | InventorySlotUI | `Assets/Prefabs/UI/InventorySlot.prefab` |
| _playerGoldText | Text | PlayerGoldText의 Text 컴포넌트 |
| _leaveButton | Button | LeaveButton의 Button 컴포넌트 |

---

### 3. BackgroundScroller 설정

**위치:** Battle 씬의 Background GameObject

**설정:**
1. Background GameObject 선택
2. `BackgroundScroller` 컴포넌트 추가

| 필드 | 타입 | 값 | 설명 |
|------|------|---|------|
| _scrollDuration | float | 0.5 | 스크롤 애니메이션 시간 (초) |
| _scaleAmount | float | 0.1 | Y축 스케일 변화량 |

**필요 컴포넌트:**
- RectTransform (필수 - Background에 있어야 함)
- BackgroundScroller (추가)

---

### 4. DungeonController 설정

**위치:** Battle 씬의 Managers GameObject

**설정:**
1. Managers GameObject 선택
2. `DungeonController` 컴포넌트 추가

| 필드 | 타입 | 연결 대상/값 | 설명 |
|------|------|-------------|------|
| _treasurePopupUI | TreasurePopupUI | TreasurePopup 프리팹 인스턴스 | 보물 팝업 |
| _shopUI | ShopUI | Shop 프리팹 인스턴스 | 상점 UI |
| _backgroundScroller | BackgroundScroller | Background의 컴포넌트 | 배경 스크롤 |
| _monsterDisplay | MonsterDisplay | MonsterDisplay 컴포넌트 | 몬스터 표시 (죽으면 숨김) |
| _battleController | BattleController | BattleController 컴포넌트 | 전투 컨트롤러 |
| _minGoldReward | int | 10 | 보물 최소 골드 |
| _maxGoldReward | int | 50 | 보물 최대 골드 |

---

### 5. BattleController 연결

**BattleController 컴포넌트 설정:**

| 필드 | 타입 | 연결 대상 |
|------|------|-----------|
| _dungeonController | DungeonController | Managers의 DungeonController 컴포넌트 |

---

### 6. Canvas 최종 구조

```
Canvas
├── EnemyInfomation (GameObject)
│   └── MonsterStatusUI (컴포넌트)
├── PlayerInfomation (GameObject)
│   └── PlayerStatusUI (컴포넌트)
├── LogUI (GameObject)
│   └── BattleLogUI (컴포넌트)
├── AttackBtn, DefenseBtn, ItemBtn (GameObject)
│   └── ActionMenuUI (컴포넌트 - 부모에)
├── SkillSlot_1, SkillSlot_2, SkillSlot_3 (GameObject)
│   └── SkillSlotUI (컴포넌트 - 부모에)
├── InventoryPanel (GameObject) - 초기 비활성화
│   └── InventoryUI (컴포넌트)
├── TreasurePopup (GameObject) - 초기 비활성화 ← 새로 추가
│   └── TreasurePopupUI (컴포넌트)
└── Shop (GameObject) - 초기 비활성화 ← 새로 추가
    └── ShopUI (컴포넌트)
```

---

### 데이터 흐름

```
몬스터 처치
    ↓
[BattleController.HandleMonsterDeath]
    ↓
[DungeonController.OnMonsterDeath]
    ↓
[EncounterSystem.GetNextEncounter]
    ├─ Battle (60%) → [BackgroundScroller] → 다음 몬스터
    ├─ Treasure (25%) → [TreasurePopupUI] → 확인 → 다음 몬스터
    └─ Shop (15%) → [ShopUI] → 나가기 → 다음 몬스터
```

### 보스 확률

| 처치 수 | 확률 |
|--------|------|
| 1~9 | 0% |
| 10 | 10% |
| 11 | 20% |
| 12 | 30% |
| ... | +10% |
| 19+ | 100% |

---

## 확인 체크리스트

- [ ] Build Settings: Title, Dungeon, Battle 씬 추가
- [ ] Title 씬: GameRoot - RootContext, GameInstaller
- [ ] CSV 변환 완료
- [ ] Addressables 빌드
- [ ] TreasurePopup 프리팹 생성 (초기 비활성화)
- [ ] Shop 프리팹 생성 (초기 비활성화)
- [ ] Background: BackgroundScroller 컴포넌트 추가
- [ ] Managers: DungeonController 컴포넌트 추가 및 연결
- [ ] BattleController: _dungeonController 연결
- [ ] Canvas: TreasurePopup, Shop 인스턴스 배치

---

> 이전 설정 기록: `Setup_archive.md`