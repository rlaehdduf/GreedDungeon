# Unity 수동 설정 가이드

## 필수 설정 (1회성)

### 1. Build Settings
- **File → Build Settings** → 씬 추가: `Title`, `Dungeon`, `Battle`

### 2. Title 씬 DI 설정
- `GameRoot` GameObject 생성
- `RootContext` + `GameInstaller` 컴포넌트 추가
- `RootContext.Installers`에 `GameInstaller` 할당

### 3. CSV 변환
- **Tools → CSV → Convert All**

### 4. Addressables 설정
- **Tools → Addressables → Set All Prefab Addresses**
- ScriptableObject 라벨 설정: `MonsterData`, `SkillData`, `EquipmentData`, `ConsumableData`, `RarityData`, `StatusEffectData`
- **Window → Asset Management → Addressables → Groups → Build → New Build**

---

## Battle 씬 구조

```
Battle.unity
├── GameRoot (필수 - DI)
│   ├── RootContext (컴포넌트)
│   └── GameInstaller (컴포넌트)
│
├── Canvas
│   ├── EnemyInfomation
│   ├── PlayerInfomation
│   ├── LogUI
│   ├── AttackBtn
│   ├── DefenseBtn
│   ├── ItemBtn
│   ├── SkillSlot_1
│   ├── SkillSlot_2
│   ├── SkillSlot_3
│   └── BattleUI ← 컴포넌트 추가
│
├── Managers
│   ├── BattleController ← 컴포넌트 추가
│   └── MonsterDisplay ← 컴포넌트 추가
│
├── Main Camera
├── EventSystem
├── Background
└── EnemySpawnPoint
```

---

## Managers 설정

### BattleController
| 필드 | 연결 대상 |
|------|-----------|
| `_monsterDisplay` | MonsterDisplay GameObject |
| `_battleUI` | Canvas (BattleUI 컴포넌트) |

### MonsterDisplay
| 필드 | 연결 대상 |
|------|-----------|
| `_spriteView` | MonsterSpriteView 컴포넌트 (자기 자신) |
| `_spriteRenderer` | EnemySpawnPoint의 SpriteRenderer |

> MonsterDisplay GameObject에 `MonsterDisplay`, `MonsterSpriteView` 컴포넌트 추가

---

## Canvas 설정

### BattleUI
| 필드 | 연결 대상 |
|------|-----------|
| `_playerStatus` | PlayerInfomation (PlayerStatusUI 컴포넌트) |
| `_monsterStatus` | EnemyInfomation (MonsterStatusUI 컴포넌트) |
| `_battleLog` | LogUI (BattleLogUI 컴포넌트) |
| `_actionMenu` | 버튼 부모 (ActionMenuUI 컴포넌트) |
| `_skillSlotUI` | SkillSlot_1~3 부모 (SkillSlotUI 컴포넌트) |

### SkillSlotUI
| 필드 | 연결 대상 |
|------|-----------|
| `_slot1` | SkillSlot_1 Button |
| `_slot2` | SkillSlot_2 Button |
| `_slot3` | SkillSlot_3 Button |
| `_icon1` | SkillSlot_1 자식 Image (아이콘) |
| `_icon2` | SkillSlot_2 자식 Image (아이콘) |
| `_icon3` | SkillSlot_3 자식 Image (아이콘) |
| `_tooltipPanel` | Tooltip Panel (초기 비활성화) |
| `_tooltipName` | Tooltip 이름 Text |
| `_tooltipDesc` | Tooltip 설명 Text |
| `_tooltipCooldown` | Tooltip 쿨타임 Text |
| `_defaultSkillIcon` | 기본 스킬 아이콘 Sprite (선택) |

> **스킬 슬롯 구조:**
> ```
> SkillSlot_1 (Button)
> └── Icon (Image) ← alpha 0으로 시작, 스킬 있으면 표시
> ```
>
> **툴팁 구조 (Canvas 하위에 배치):**
> ```
> TooltipPanel (GameObject, 초기 비활성화)
> ├── (Vertical Layout Group 컴포넌트)
> ├── (Content Size Fitter 컴포넌트 - Horizontal: Preferred, Vertical: Preferred)
> ├── NameText (Text) ← 스킬 이름
> ├── DescText (Text) ← 설명
> └── CooldownText (Text) ← "쿨타임: 1/3"
> ```
>
> **툴팁 Panel 설정 (Inspector):**
> 1. `Vertical Layout Group` 컴포넌트 추가
>    - Padding: Left 30, Right 30, Top 30, Bottom 50
>    - Spacing: 4
>    - Child Alignment: Upper Left
>    - Child Force Expand: Width ✓, Height ✗
> 2. `Content Size Fitter` 컴포넌트 추가
>    - Horizontal Fit: Preferred Size
>    - Vertical Fit: Preferred Size
> 3. `Image` 컴포넌트 추가 (배경)
> 
> **Text 설정:**
> - NameText: Font Size 16, Bold, Alignment: Left
> - DescText: Font Size 12, Alignment: Left
> - CooldownText: Font Size 12, Alignment: Left
>
> 마우스를 슬롯에 올리면 툴팁이 마우스를 따라다님

### PlayerStatusUI (PlayerInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_mpBar` | MP Slider |
| `_mpText` | MP Text |
| `_debuffContainer` | 디버프 슬롯 부모 Transform |
| `_debuffSlotPrefab` | StatusEffectSlotUI 프리팹 |
| `_buffContainer` | 버프 슬롯 부모 Transform |
| `_buffSlotPrefab` | StatusEffectSlotUI 프리팹 |

> Player 이름은 고정 "Player", 레벨 없음
> 슬롯 구조: Image (아이콘) + Text (지속시간 표시)
> 버프/디버프 동적 생성: 필요한 만큼 자동 생성됨

### MonsterStatusUI (EnemyInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_nameText` | 이름 Text |
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_elementIcon` | 속성 아이콘 Image |
| `_debuffContainer` | 디버프 슬롯 부모 Transform |
| `_debuffSlotPrefab` | StatusEffectSlotUI 프리팹 |

> Monster 레벨 없음
> 디버프 아이콘: Addressables에서 로드 (`StatusEffectDataSO.IconAddress`)
> 디버프 동적 생성: 필요한 만큼 자동 생성됨

### BattleLogUI (LogUI에 추가)

**구조:**
```
LogUI (ScrollRect)
├── Viewport (Image + Rect Mask 2D)
│   └── Content (Vertical Layout Group + Content Size Fitter)
│       └── (Log 프리팹들이 여기에 생성됨)
```

**1. LogUI 설정:**
| 컴포넌트 | 설정 |
|---------|------|
| RectTransform | Anchor: Top-Center, Size: (600, 200) |
| ScrollRect | Vertical ✓, Content: Content Transform |
| Vertical Layout Group | Child Alignment: Middle Center |

**2. Viewport 설정 (LogUI 자식):**
| 컴포넌트 | 설정 |
|---------|------|
| RectTransform | Stretch/Stretch |
| Image | Color: (0,0,0,100) 또는 투명 |
| Rect Mask 2D | 추가 |

**3. Content 설정 (Viewport 자식):**
| 컴포넌트 | 설정 |
|---------|------|
| RectTransform | Stretch/Stretch, Left/Top/Right/Bottom = 0 |
| Vertical Layout Group | Spacing: 5, Child Force Expand Width ✓ |
| Content Size Fitter | Vertical: Preferred Size |

**4. BattleLogUI 컴포넌트 (LogUI에 추가):**
| 필드 | 연결 대상 |
|------|-----------|
| `_logContainer` | Content Transform |
| `_logEntryPrefab` | Log 프리팹 |
| `_scrollRect` | LogUI의 ScrollRect |

**5. Log 프리팹 설정:**
```
Log (GameObject)
└── Text (Component)
```
| Text 설정 | 값 |
|----------|---|
| Font Size | 14 |
| Alignment | Middle Center |
| Horizontal Overflow | Overflow |
| Vertical Overflow | Truncate |

> **주의:** `_scrollRect`가 null이면 로그가 화면 밖으로 나감

### ActionMenuUI (버튼 부모에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_attackButton` | AttackBtn |
| `_defendButton` | DefenseBtn |
| `_itemButton` | ItemBtn |

> 스킬 슬롯 3개는 SkillSlotUI 사용 (무기/갑옷/악세서리 스킬)

### InventoryUI (InventoryPanel에 추가)
        | 필드 | 연결 대상 |
        |------|-----------|
        | `_statsText` | 스탯 Text (HP/MP/ATK/DEF/SPD/CRIT) |
        | `_weaponSlot` | WeaponSlot (EquipSlotUI 컴포넌트) |
        | `_armorSlot` | ArmorSlot (EquipSlotUI 컴포넌트) |
        | `_accessorySlot` | AccessorySlot (EquipSlotUI 컴포넌트) |
        | `_inventoryGrid` | 인벤토리 슬롯 부모 Transform |
        | `_slotPrefab` | InventorySlotUI 프리팹 |
        | `_goldText` | 골드 Text |
        | `_tooltip` | ItemTooltipUI 컴포넌트 |
        | `_dropPopup` | ConfirmDropPopup 컴포넌트 |
        | `_closeArea` | CloseArea (투명 배경 Image) |

        > **인벤토리 패널 구조:**
        > ```
        > InventoryPanel (GameObject, 초기 비활성화)
        > ├── CloseArea (Image, Color.a=0, Raycast Target=true) ← 외부 클릭 닫기
        > │   └── InventoryContent (실제 UI)
        > │       ├── StatsText (우상단, 왼쪽 정렬)
        > │       ├── EquipSlots
        > │       │   ├── WeaponSlot (EquipSlotUI)
        > │       │   ├── ArmorSlot (EquipSlotUI)
        > │       │   └── AccessorySlot (EquipSlotUI)
        > │       ├── InventoryGrid (Grid Layout Group)
        > │       │   └── (InventorySlotUI 프리팹으로 21개 생성)
        > │       ├── GoldText
        > │       ├── ItemTooltipUI (단일 툴팁)
        > │       └── ConfirmDropPopup
        > ```
        >
        > **CloseArea 설정:**
        > - Image 컴포넌트 추가 (Color: R=0, G=0, B=0, A=0)
        > - Raycast Target: true
        > - Anchors: Stretch/Stretch (전체 화면)
        > - 클릭 시 인벤토리 닫기
        >
        > **ESC 키로도 인벤토리 닫기 가능**

> **Grid Layout Group 설정:**
> - Cell Size: X=150, Y=150
> - Spacing: X=20, Y=20
> - Constraint: Fixed Column Count = 7
> - Constraint Count: 7

### InventorySlotUI 프리팹 구조
| 필드 | 연결 대상 |
|------|-----------|
| `_iconImage` | 아이콘 Image |
| `_quantityText` | 수량 Text |
| `_backgroundImage` | 배경 Image (등급 색상) |

> **슬롯 프리팹 구조:**
> ```
> InventorySlotUI (GameObject)
> ├── Background (Image) ← 등급 색상 표시
> ├── Icon (Image) ← 아이템 아이콘
> └── Quantity (Text) ← 소모품 수량
> ```
>
> **주의:** 툴팁은 InventoryUI에서 단일로 관리됨

### EquipSlotUI 구조
| 필드 | 연결 대상 |
|------|-----------|
| `_iconImage` | 아이콘 Image |
| `_backgroundImage` | 배경 Image |
| `_equippedLabel` | 장착 표시 GameObject |
| `_tooltip` | ItemTooltipUI 컴포넌트 |

> **장비 슬롯 구조:**
> ```
> EquipSlotUI (GameObject)
> ├── Background (Image) ← 등급 색상 표시
> ├── Icon (Image) ← 장비 아이콘
> └── EquippedLabel (GameObject) ← 장착 중 표시
> ```

### ItemTooltipUI 구조
        | 필드 | 연결 대상 |
        |------|-----------|
        | `_nameText` | 아이템 이름 Text |
        | `_descriptionText` | 설명 Text |
        | `_statsText` | 스탯 Text |
        | `_skillIcon` | 스킬 아이콘 Image |
        | `_skillTooltipPanel` | 스킬 툴팁 패널 GameObject |
        | `_skillTooltipName` | 스킬 이름 Text |
        | `_skillTooltipDesc` | 스킬 설명 Text |

        > **툴팁 구조:**
        > ```
        > ItemTooltipUI (GameObject, 초기 비활성화)
        > ├── CanvasGroup (blocksRaycasts=false) ← 포인터 이벤트 투과
        > ├── Background (Image)
        > └── Content (Vertical Layout Group)
        >     ├── NameText
        >     ├── DescriptionText
        >     ├── StatsText
        >     ├── SkillIcon (Image)
        >     └── SkillTooltipPanel (초기 비활성화)
        >         ├── SkillTooltipName
        >         └── SkillTooltipDesc
        > ```
        >
        > **CanvasGroup 필수 설정:**
        > - `blocksRaycasts = false` (코드에서 자동 추가되지만 Inspector에서 확인)
        > - 이 설정이 없으면 툴팁이 포인터 이벤트를 가로채서 슬롯 hover가 끊김

### ConfirmDropPopup 구조
| 필드 | 연결 대상 |
|------|-----------|
| `_messageText` | 메시지 Text |
| `_cancelButton` | 취소 Button |
| `_confirmButton` | 확인 Button |

> **팝업 구조:**
> ```
> ConfirmDropPopup (GameObject, 초기 비활성화)
> ├── Background (Image)
> └── Content
>     ├── MessageText
>     └── Buttons
>         ├── CancelButton
>         └── ConfirmButton
> ```

### BattleUI 인벤토리 설정
| 필드 | 연결 대상 |
|------|-----------|
| `_inventoryUI` | InventoryPanel (InventoryUI 컴포넌트) |

> ItemBtn 클릭 시 `ToggleInventory()` 호출로 인벤토리 열기/닫기

---

## 스킬 시스템

### 스킬 획득
- 장비 장착 시 `SkillPoolType`별 랜덤 스킬 획득
- 슬롯 최대 3개 (무기/갑옷/악세서리)

### 스킬 타입별 동작
| 타입 | 동작 |
|------|------|
| Common/Melee/Magic | 적에게 데미지 |
| Buff | 공격력/방어력/속도 증가 또는 HP 회복 |
| Passive | 슬롯 장착 중 스탯 영구 증가 |

### 쿨다운
- 전역 쿨다운 (전투 간 유지)
- 턴 종료 시 1 감소
- UI에 남은 턴 수 표시

### DI 등록
```
ISkillManager → SkillManager (Singleton)
```

---

## 확인 체크리스트

- [ ] Build Settings에 3개 씬 추가
- [ ] Title 씬 DI 설정
- [ ] CSV 변환 완료
- [ ] Addressables 주소/라벨 설정 및 빌드
- [ ] Battle 씬: GameRoot - RootContext, GameInstaller
- [ ] Battle 씬: Managers - BattleController, MonsterDisplay
- [ ] Battle 씬: Canvas - BattleUI, SkillSlotUI, PlayerStatusUI, MonsterStatusUI, BattleLogUI, ActionMenuUI

---

## 2026-03-25: Rarity Color 적용

### 해야 할 일
1. Unity Editor에서 `Tools → CSV → Convert Rarities` 실행
2. 변환 완료 로그 확인 ("Rarity 변환 완료: 5개")
3. 테스트: 인벤토리에서 아이템 툴팁 확인

### 색상 매핑
| Rarity | Color | Hex |
|--------|-------|-----|
| Common | 흰색 | FFFFFF |
| UnCommon | 녹색 | 00FF00 |
| Rare | 파랑 | 0080FF |
| Epic | 보라 | A020F0 |
| Legend | 금색 | FFD700 |

---

## 파일 구조

```
Assets/Scripts/
├── Core/
│   ├── GameInstaller.cs      ← DI 등록
│   ├── Services.cs
│   └── IGameDataManager.cs
│
├── Skill/
│   ├── ISkillManager.cs      ← 스킬 관리 인터페이스
│   └── SkillManager.cs       ← 풀, 쿨다운, 실행
│
├── Combat/
│   ├── BattleManager.cs      ← 전투 관리
│   ├── BattleController.cs   ← 스킬 사용
│   ├── DamageCalculator.cs
│   └── TurnManager.cs
│
├── Character/
│   ├── Player.cs             ← 패시브 적용, 스킬 획득
│   ├── Monster.cs
│   └── BattleEntity.cs
│
└── UI/Battle/
    ├── BattleUI.cs
    ├── SkillSlotUI.cs        ← 쿨다운 표시
    ├── PlayerStatusUI.cs
    ├── MonsterStatusUI.cs
    ├── BattleLogUI.cs
    └── ActionMenuUI.cs
```