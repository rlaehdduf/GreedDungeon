# Setup 아카이브

이전 작업 기록들을 보관합니다.

---

## 2026-03-25: Environment 레이어 제거

1. 모든 StatusEffectSlot 프리팹 Layer를 UI로 변경
2. **Edit → Project Settings → Tags and Layers** → Environment 레이어 이름 빈 문자열로 지우기

---

## 2026-03-25: 버프/디버프 Container 왼쪽 정렬

Container 프리팹 설정:
- **RectTransform**: Anchor Min/Max (0, 0.5), Pivot (0, 0.5)
- **HorizontalLayoutGroup**: ChildAlignment = Middle Left (3)

---

## 2026-03-25: 전투 비주얼 시스템

### Unity 설정

| 컴포넌트 | 위치 | 연결 |
|---------|------|------|
| DamageTextUI | Canvas/DamageTextContainer | BattleUI._damageTextUI |
| AttackEffectUI | Canvas/AttackEffect (Image) | BattleUI._attackEffectUI |
| DebuffVignetteUI | Canvas/DebuffVignette (Image) | BattleUI._debuffVignetteUI |

### 이펙트 Addressables
- `Effects/Neutral`, `Effects/Melee`, `Effects/Magic`

### 디버프 색상
| 디버프 | 색상 |
|-------|------|
| Burn | #FF4444 |
| Poison | #44FF44 |
| Stun | #FFFF44 |

---

## 2026-03-25: Rarity Color 적용

`Tools → CSV → Convert Rarities` 실행

| Rarity | Hex |
|--------|-----|
| Common | FFFFFF |
| UnCommon | 00FF00 |
| Rare | 0080FF |
| Epic | A020F0 |
| Legend | FFD700 |

---

## 상세 UI 설정 (참고용)

### SkillSlotUI
| 필드 | 연결 |
|------|------|
| _slot1~3 | SkillSlot Button |
| _icon1~3 | 자식 Image |
| _tooltipPanel | Tooltip Panel |

### PlayerStatusUI
| 필드 | 연결 |
|------|------|
| _hpBar, _mpBar | Slider |
| _hpText, _mpText | Text |
| _debuffContainer, _buffContainer | Transform |
| _debuffSlotPrefab, _buffSlotPrefab | StatusEffectSlot.prefab |

### MonsterStatusUI
| 필드 | 연결 |
|------|------|
| _nameText | Text |
| _hpBar | Slider |
| _elementIcon | Image |
| _debuffSlot | StatusEffectSlotUI |

### BattleLogUI
| 필드 | 연결 |
|------|------|
| _logContainer | Content Transform |
| _logEntryPrefab | Log 프리팹 |
| _scrollRect | ScrollRect |

### InventoryUI
| 필드 | 연결 |
|------|------|
| _statsText | 스탯 Text |
| _weaponSlot, _armorSlot, _accessorySlot | EquipSlotUI |
| _inventoryGrid | Transform |
| _slotPrefab | InventorySlotUI |
| _goldText | Text |
| _tooltip | ItemTooltipUI |
| _dropPopup | ConfirmDropPopup |
| _closeArea | Image (투명) |

### ItemTooltipUI
| 필드 | 연결 |
|------|------|
| _nameText, _descriptionText, _statsText | Text |
| _skillIcon | Image |
| _skillTooltipPanel | GameObject |

> CanvasGroup.blocksRaycasts = false 필수