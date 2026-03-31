# Full Korean to English Translation Design

## Overview

Translate all Korean text in the project to English for WebGL build compatibility.
Korean fonts break in WebGL builds on GitHub, requiring full translation.

## Translation Scope

### Data Files (CSV/xlsm)

| File | Content | Count |
|------|---------|-------|
| EquipmentData.csv | Names, descriptions | 18 items |
| MonsterData.csv | Names, elements | 5 monsters |
| MonsterSkill.csv | Names, descriptions | 11 skills |
| SkillData.csv | Names, descriptions | 19 skills |
| ConsumableData.csv | Names, descriptions | 10 items |
| RarityData.csv | (check needed) | - |
| StatusEffect.csv | (check needed) | - |
| PlayerData.csv | (check needed) | - |
| GameData.xlsm | Excel source for all CSV | 1 file |

### C# Files

Files with Korean text (from grep analysis):

| File | Korean Content |
|------|----------------|
| BattleSimulator.cs | Monster names, UI text, stats |
| BattleManager.cs | Battle log text (Victory, Defeat, etc.) |
| BattleUI.cs | Victory/Defeat/Game Over text |
| BattleEntity.cs | Debug logs |
| Player.cs | Debug logs |
| ItemTooltipUI.cs | Tooltip text (Effect, Quantity, Heal, etc.) |
| ShopUI.cs | Debug logs (Gold insufficient, etc.) |
| CSVConverter.cs | Debug logs, Element/BuffType mapping |
| ScriptableObjectAddressablesSetter.cs | Debug logs |
| GameDataManager.cs | Debug logs |
| PlayerDataSO.cs | Header comments |
| WindowAspectRatio.cs | Comments |

### Unity Scenes/Prefabs

TextMeshPro objects requiring manual update in Unity Editor:
- Battle.unity
- Title.unity
- Prefabs (Shop, TreasurePopup, InventorySlotUI, etc.)

## Translation Style

**General RPG Style** - Standard, clean naming convention.

Examples:
- 슬라임 → Slime
- 역병쥐 → Plague Rat
- 거미 → Spider
- 해골 → Skeleton
- 켈베로스 → Cerberus
- 강타 → Power Strike
- 연속공격 → Double Strike
- 독가스 → Poison Gas
- 분노 → Rage
- 재생 → Regeneration
- 승리! → Victory!
- 패배... → Defeat...
- 게임 오버! → Game Over!
- 방어 → Defend
- 디버프 해제 → Remove Debuff
- HP 회복 → HP Restored
- 불/물/풀 → Fire/Water/Grass
- 힘의물약 → Potion of Strength
- 철의물약 → Potion of Iron
- 회복포션 → Healing Potion

## Implementation Order

1. **CSV Files** - Translate all data files first
2. **xlsm File** - Update GameData.xlsm (Excel source)
3. **C# Code** - Translate UI text, debug logs, comments
4. **Scene/Prefab Text Objects** - List for manual Unity Editor update
5. **Rebuild** - Run CSV converter → Addressables rebuild

## Success Criteria

- All Korean text translated to English
- WebGL build displays correctly without font issues
- Game playable on GitHub Pages without Korean font requirements