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
| `_actionMenu` | AttackBtn/DefenseBtn/ItemBtn 부모 (ActionMenuUI 컴포넌트) |
| `_skillSlotUI` | SkillSlot_1 부모 (SkillSlotUI 컴포넌트) |

### SkillSlotUI
| 필드 | 연결 대상 |
|------|-----------|
| `_slot1` | SkillSlot_1 Button |
| `_slot2` | SkillSlot_2 Button |
| `_slot3` | SkillSlot_3 Button |

### PlayerStatusUI (PlayerInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_nameText` | 이름 Text |
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |
| `_mpBar` | MP Slider (있는 경우) |
| `_mpText` | MP Text (있는 경우) |

### MonsterStatusUI (EnemyInfomation에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_nameText` | 이름 Text |
| `_hpBar` | HP Slider |
| `_hpText` | HP Text |

### BattleLogUI (LogUI에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_logText` | Log Text |

### ActionMenuUI (버튼 부모에 추가)
| 필드 | 연결 대상 |
|------|-----------|
| `_attackButton` | AttackBtn |
| `_defendButton` | DefenseBtn |
| `_itemButton` | ItemBtn |

---

## 확인 체크리스트

- [ ] Build Settings에 3개 씬 추가
- [ ] Title 씬 DI 설정
- [ ] CSV 변환 완료
- [ ] Addressables 주소/라벨 설정 및 빌드
- [ ] Battle 씬: Managers - BattleController, MonsterDisplay
- [ ] Battle 씬: Canvas - BattleUI, SkillSlotUI, PlayerStatusUI, MonsterStatusUI, BattleLogUI, ActionMenuUI