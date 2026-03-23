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
| `_cooldownText1` | SkillSlot_1 쿨다운 Text (선택) |
| `_cooldownText2` | SkillSlot_2 쿨다운 Text (선택) |
| `_cooldownText3` | SkillSlot_3 쿨다운 Text (선택) |

> 스킬은 장비 장착 시 SkillPoolType별 랜덤 획득
> 쿨다운은 전역 (전투 간 유지)

### PlayerStatusUI (PlayerInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_mpBar` | MP Slider |
| `_mpText` | MP Text |
| `_statusEffectsContainer` | DeBuff 아이콘 부모 Transform |
| `_statusEffectPrefab` | DeBuff 아이콘 프리팹 (Text "Count" 포함) |
| `_buffsContainer` | Buff 아이콘 부모 Transform |
| `_buffPrefab` | Buff 아이콘 프리팹 (Text "Count", "Value" 포함) |

> Player 이름은 고정 "Player", 레벨 없음
> 프리팹 구조: Image (아이콘) + Text (이름에 "Count" 또는 "Duration" 포함 시 지속시간 표시)

### MonsterStatusUI (EnemyInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_nameText` | 이름 Text |
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_elementIcon` | 속성 아이콘 Image |

> Monster 레벨, 상태이상 없음

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

## 확인 체크리스트

- [ ] Build Settings에 3개 씬 추가
- [ ] Title 씬 DI 설정
- [ ] CSV 변환 완료
- [ ] Addressables 주소/라벨 설정 및 빌드
- [ ] Battle 씬: GameRoot - RootContext, GameInstaller
- [ ] Battle 씬: Managers - BattleController, MonsterDisplay
- [ ] Battle 씬: Canvas - BattleUI, SkillSlotUI, PlayerStatusUI, MonsterStatusUI, BattleLogUI, ActionMenuUI