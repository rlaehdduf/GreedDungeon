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
| `_debuffSlots` | DeBuff 슬롯들 (StatusEffectSlotUI 컴포넌트) |
| `_buffSlots` | Buff 슬롯들 (StatusEffectSlotUI 컴포넌트) |

> Player 이름은 고정 "Player", 레벨 없음
> 슬롯 구조: Image (아이콘) + Text (이름에 "Count" 포함 → 지속시간 표시)
> 버프/디버프 없으면 슬롯 비활성화, 발생 시 활성화하고 아이콘 로드

### MonsterStatusUI (EnemyInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_nameText` | 이름 Text |
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_elementIcon` | 속성 아이콘 Image |
| `_debuffSlots` | DeBuff 슬롯들 (StatusEffectSlotUI 컴포넌트) |

> Monster 레벨 없음
> 디버프 아이콘: Addressables에서 로드 (`StatusEffectDataSO.IconAddress`)
> 디버프 없으면 슬롯 비활성화, 발생 시 활성화하고 아이콘 로드 + Count 표시

### BattleLogUI (LogUI에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_logContainer` | 로그 엔트리 부모 Transform |
| `_logEntryPrefab` | 로그 텍스트 프리팹 (Text 컴포넌트 포함) |
| `_scrollRect` | ScrollRect (선택) |

### ActionMenuUI (버튼 부모에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_attackButton` | AttackBtn |
| `_defendButton` | DefenseBtn |
| `_itemButton` | ItemBtn |

> 스킬 슬롯 3개는 SkillSlotUI 사용 (무기/갑옷/악세서리 스킬)
> 인벤토리 UI는 미구현

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