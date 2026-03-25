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
├── GameRoot (RootContext, GameInstaller)
├── Canvas
│   ├── EnemyInfomation, PlayerInfomation, LogUI
│   ├── AttackBtn, DefenseBtn, ItemBtn
│   ├── SkillSlot_1~3
│   └── BattleUI 컴포넌트
├── Managers
│   ├── BattleController
│   ├── MonsterDisplay
│   └── DungeonController ← 새로 추가
├── Background (BackgroundScroller) ← 새로 추가
└── EnemySpawnPoint
```

---

## 컴포넌트 연결

### BattleController
| 필드 | 연결 |
|------|------|
| _monsterDisplay | MonsterDisplay |
| _battleUI | Canvas (BattleUI) |
| _dungeonController | DungeonController |

### BattleUI
| 필드 | 연결 |
|------|------|
| _playerStatus | PlayerStatusUI |
| _monsterStatus | MonsterStatusUI |
| _battleLog | BattleLogUI |
| _actionMenu | ActionMenuUI |
| _skillSlotUI | SkillSlotUI |
| _inventoryUI | InventoryUI |

### PlayerStatusUI
| 필드 | 연결 |
|------|------|
| _hpBar, _mpBar | Slider |
| _debuffContainer, _buffContainer | Transform |
| _debuffSlotPrefab, _buffSlotPrefab | StatusEffectSlot.prefab |

### InventoryUI
| 필드 | 연결 |
|------|------|
| _inventoryGrid | Transform |
| _slotPrefab | InventorySlotUI.prefab |
| _tooltip | ItemTooltipUI |

---

## 스킬 시스템

- 장비 장착 시 랜덤 스킬 획득 (최대 3개)
- 타입: Common/Melee/Magic(데미지), Buff(버프), Passive(영구 스탯)
- 쿨다운: 턴 종료 시 1 감소

---

## 2026-03-25: 던전 방 전환 시스템

### 개요
몬스터 처치 → 조우 결정(전투/보물/상점) → 다음 방

### 생성된 파일
```
Assets/Scripts/Dungeon/
├── DungeonState.cs, EncounterType.cs, DungeonProgress.cs
├── EncounterSystem.cs, BackgroundScroller.cs
├── DungeonController.cs
└── UI/TreasurePopupUI.cs, ShopUI.cs
```

### Unity 설정

#### 1. TreasurePopup 프리팹
**위치:** `Assets/Prefabs/UI/TreasurePopup.prefab`

```
TreasurePopup (초기 비활성화)
├── CanvasGroup
├── Image (배경)
└── Content
    ├── GoldText
    ├── ItemContainer
    └── ConfirmButton
```

**컴포넌트 설정:**
| 필드 | 연결 |
|------|------|
| _goldText | GoldText |
| _itemContainer | ItemContainer |
| _itemSlotPrefab | InventorySlotUI.prefab |
| _confirmButton | ConfirmButton |

---

#### 2. Shop 프리팹
**위치:** `Assets/Prefabs/UI/Shop.prefab`

```
Shop (초기 비활성화)
├── CanvasGroup
├── Image
└── Content
    ├── LeftPanel (상점 슬롯 3개)
    │   ├── ShopSlotsContainer
    │   └── PlayerGoldText
    ├── RightPanel (판매용 인벤토리)
    │   └── PlayerInventoryContainer
    └── LeaveButton
```

**컴포넌트 설정:**
| 필드 | 연결 |
|------|------|
| _shopSlotsContainer | ShopSlotsContainer |
| _slotPrefab | InventorySlotUI.prefab |
| _playerInventoryContainer | PlayerInventoryContainer |
| _playerSlotPrefab | InventorySlotUI.prefab |
| _playerGoldText | PlayerGoldText |
| _leaveButton | LeaveButton |

---

#### 3. BackgroundScroller
Background GameObject에 컴포넌트 추가:
- _scrollDuration: 0.5
- _scaleAmount: 0.1

---

#### 4. DungeonController
Managers GameObject에 컴포넌트 추가:

| 필드 | 연결 |
|------|------|
| _treasurePopupUI | TreasurePopup |
| _shopUI | Shop |
| _backgroundScroller | Background |
| _battleController | BattleController |
| _minGoldReward | 10 |
| _maxGoldReward | 50 |

---

#### 5. BattleController 연결
| 필드 | 연결 |
|------|------|
| _dungeonController | DungeonController |

---

### 데이터 흐름
```
몬스터 처치 → DungeonController.OnMonsterDeath
    ├─ Battle (60%) → 다음 몬스터
    ├─ Treasure (25%) → TreasurePopup → 다음 몬스터
    └─ Shop (15%) → ShopUI → 다음 몬스터
```

### 보스 확률
| 처치 수 | 확률 |
|--------|------|
| 1~9 | 0% |
| 10 | 10% |
| 19+ | 100% |

---

## 확인 체크리스트

- [ ] Build Settings 씬 추가
- [ ] Title 씬 DI 설정
- [ ] CSV 변환
- [ ] Addressables 빌드
- [ ] Battle 씬 컴포넌트 연결
- [ ] TreasurePopup, Shop 프리팹 생성
- [ ] DungeonController, BackgroundScroller 추가

---

> 이전 설정 기록: `Setup_archive.md`