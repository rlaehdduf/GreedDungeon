# Full Korean to English Translation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Translate all Korean text in the GreedDungeon project to English for WebGL build compatibility.

**Architecture:** Direct file edits across CSV data files, C# source files, and Unity ScriptableObject headers. No structural changes — only text replacement.

**Tech Stack:** Unity C#, CSV data files, Excel (.xlsm), TextMeshPro (Unity scenes/prefabs)

---

### Task 1: Translate CSV Data Files

**Files:**
- Modify: `Assets/EditorData/Data/csv/EquipmentData.csv`
- Modify: `Assets/EditorData/Data/csv/MonsterData.csv`
- Modify: `Assets/EditorData/Data/csv/MonsterSkill.csv`
- Modify: `Assets/EditorData/Data/csv/SkillData.csv`
- Modify: `Assets/EditorData/Data/csv/ConsumableData.csv`

**Note:** StatusEffect.csv, RarityData.csv, PlayerData.csv are already English — skip these.

#### EquipmentData.csv Translation Map

| Line | Korean | English |
|------|--------|---------|
| 2 | 막대기 | Wooden Stick |
| 2 | 길 가다보면 흔히 보이는 나무 막대기다 | A common wooden stick found along the road |
| 3 | 검 | Sword |
| 3 | 날이 무뎌진 낡은 검이다 | An old sword with a dull blade |
| 4 | 방패 | Shield |
| 4 | 오랫동안 방치된듯 거미줄이 쳐진 먼지많은 방패다 | A dusty shield covered in cobwebs, long abandoned |
| 5 | 완드 | Wand |
| 5 | 마법사의 시체에서 주운 완드다 아직까진 보석의 빛이 꺼지지않았다 | A wand looted from a mage's corpse. The gem still glows faintly |
| 6 | 도끼 | Axe |
| 6 | 평범한 사람이 휘두르면 바로 지칠것같은 커다란 도끼다 | A massive axe that would exhaust an ordinary person |
| 7 | 대검 | Greatsword |
| 7 | 양손으로 쥐는 거대한 검이다 | A colossal sword wielded with both hands |
| 8 | 마법지팡이 | Magic Staff |
| 8 | 고대 마법사가 사용하던 지팡이다 | A staff once wielded by an ancient mage |
| 9 | 가죽갑옷 | Leather Armor |
| 9 | 동물의 가죽으로 만들어진 갑옷이다 | Armor crafted from animal hide |
| 10 | 사슬갑옷 | Chain Armor |
| 10 | 사슬들을 꼬아놓은 갑옷이다 움직일때마다 짤랑인다 | Armor woven from chains. Clinks with every movement |
| 11 | 강철갑옷 | Plate Armor |
| 11 | 너무 단단하고 무거운 갑옷이다 갑옷을입고 생활하긴 힘들것같다 | Hard and heavy armor. Difficult to live in |
| 12 | 가시갑옷 | Spike Armor |
| 12 | 최선의 방어는 공격이다 갑옷에 가시를박아 반사 피해를주는 갑옷이다 | The best defense is offense. Armor studded with spikes for reflected damage |
| 13 | 깃털갑옷 | Feather Armor |
| 13 | 엄청난 강도의 깃털을 엮은 갑옷이다 매우가벼운게 특징이다 | Armor woven from incredibly strong feathers. Remarkably lightweight |
| 14 | 드래곤갑옷 | Dragon Armor |
| 14 | 드래곤의 비늘로 만든 전설의 갑옷이다 | Legendary armor crafted from dragon scales |
| 15 | 반지 | Ring |
| 15 | 녹이 슬어있는 반지다 | A rusted ring |
| 16 | 목걸이 | Necklace |
| 16 | 목에 걸면 파상풍에 걸릴것같은 목걸이다 | A necklace that looks like it could give you tetanus |
| 17 | 망토 | Cloak |
| 17 | 망토의 역할을 하지못하는 찢어진 망토다 | A tattered cloak that barely serves its purpose |
| 18 | 보석 | Jewel |
| 18 | 일반 보석보다는 못생긴 수요가 없을것같은 보석이다 | An ugly jewel with little market demand |
| 19 | 왕관 | Crown |
| 19 | 누군진 몰라도 왕국에 들고가면 치안대로 잡혀갈듯이 정교하게 만들어진 왕관이다 | An ornate crown — bring it to any kingdom and you'll be arrested |
| 20 | 용의반지 | Dragon Ring |
| 20 | 용의 심장으로 만든 전설의 반지다 | A legendary ring forged from a dragon's heart |

#### MonsterData.csv Translation Map

| Line | Korean | English |
|------|--------|---------|
| 2 | 슬라임 | Slime |
| 2 | 풀 | Grass |
| 3 | 역병쥐 | Plague Rat |
| 3 | 무 | None |
| 4 | 거미 | Spider |
| 4 | 풀 | Grass |
| 5 | 해골 | Skeleton |
| 5 | 불 | Fire |
| 6 | 켈베로스 | Cerberus |
| 6 | 불 | Fire |

#### MonsterSkill.csv Translation Map

| Line | Korean | English |
|------|--------|---------|
| 2 | 강타 | Power Strike |
| 2 | 강한공격 | A powerful blow |
| 3 | 연속공격 | Double Strike |
| 3 | 빠르게 한번더 공격 | A rapid follow-up attack |
| 4 | 독가스 | Poison Gas |
| 4 | 독이 포함된 안개를 방사 | Releases a toxic mist |
| 5 | 분노 | Rage |
| 5 | 공격력이 올라감 | Attack power increases |
| 6 | 재생 | Regeneration |
| 6 | 체력이 회복됨 | Restores health |
| 7 | 분열 | Fission |
| 7 | 체력이 많이 회복됨 | Restores a large amount of health |
| 8 | 역병 | Plague |
| 8 | 독에걸림 | Inflicts poison |
| 9 | 독침 | Venom Sting |
| 9 | 독에걸림 | Inflicts poison |
| 10 | 강력한일격 | Devastating Blow |
| 10 | 엄청 강하게 내려침 | Strikes with tremendous force |
| 11 | 지옥불 | Hellfire |
| 11 | 너무 뜨거운 불을 내뿜음 | Breathes scorching flames |

#### SkillData.csv Translation Map

| Line | Korean | English |
|------|--------|---------|
| 2 | 강타 | Power Strike |
| 2 | 적에게 강한 한방 | A powerful strike against the enemy |
| 3 | 이중타격 | Double Strike |
| 3 | 적에게 두번 공격 | Strikes the enemy twice |
| 4 | 가로베기 | Wide Slash |
| 4 | 적들에게 넓은 공격 | A sweeping attack against enemies |
| 5 | 이중베기 | Double Slash |
| 5 | 적에게 두번 공격 | Strikes the enemy twice |
| 6 | 십자베기 | Cross Slash |
| 6 | 적에게 강한 공격 | A powerful strike against the enemy |
| 7 | 마나볼 | Mana Bolt |
| 7 | 적에게 마력 공격 | A magical attack against the enemy |
| 8 | 파이어볼 | Fireball |
| 8 | 적에게 화상 공격 | A burning attack against the enemy |
| 9 | 얼음비 | Ice Rain |
| 9 | 적들에게 광역 공격 | A wide-area attack against enemies |
| 10 | 공격력증가 | Attack Boost |
| 10 | 공격력이 5 증가합니다 | Attack increases by 5 |
| 11 | 방어력증가 | Defense Boost |
| 11 | 방어력이 5 증가합니다 | Defense increases by 5 |
| 12 | 체력증가 | Vitality Boost |
| 12 | 체력이 20 증가합니다 | HP increases by 20 |
| 13 | 속도증가 | Speed Boost |
| 13 | 속도가 3 증가합니다 | Speed increases by 3 |
| 14 | 공격력버프 | Attack Buff |
| 14 | 공격력이 30% 증가합니다 | Attack increases by 30% |
| 15 | 방어력버프 | Defense Buff |
| 15 | 방어력이 40% 증가합니다 | Defense increases by 40% |
| 16 | 속도버프 | Speed Buff |
| 16 | 속도가 5 증가합니다 | Speed increases by 5 |
| 17 | 회복 | Recovery |
| 17 | 최대체력 25%만큼 회복합니다 | Restores 25% of max HP |
| 18 | 메테오 | Meteor |
| 18 | 하늘에서 메테오를 소환한다 | Summons a meteor from the sky |
| 19 | 분노의일격 | Fury Strike |
| 19 | 분노를 담은 강력한 일격 | A powerful strike fueled by rage |
| 20 | 연속난타 | Combo Rush |
| 20 | 적에게 5번 연속 공격 | Strikes the enemy 5 times in succession |

#### ConsumableData.csv Translation Map

| Line | Korean | English |
|------|--------|---------|
| 2 | 회복포션(소) | Healing Potion (S) |
| 2 | 체력을 소량 회복한다 | Restores a small amount of HP |
| 3 | 회복포션(중) | Healing Potion (M) |
| 3 | 체력을 적당량 회복한다 | Restores a moderate amount of HP |
| 4 | 회복포션(대) | Healing Potion (L) |
| 4 | 체력을 대량 회복한다 | Restores a large amount of HP |
| 5 | 해독제 | Antidote |
| 5 | 모든 디버프를 해제한다 | Removes all debuffs |
| 6 | 힘의물약 | Potion of Strength |
| 6 | 공격력이 일정비율 증가한다 | Increases attack by a certain ratio |
| 7 | 철의물약 | Potion of Iron |
| 7 | 방어력이 일정비율 증가한다 | Increases defense by a certain ratio |
| 8 | 저주의물약 | Potion of Curse |
| 8 | 적에게 독 디버프를 부여한다 | Inflicts poison debuff on the enemy |
| 9 | 화염병 | Fire Bomb |
| 9 | 적에게 화상 디버프를 부여한다 | Inflicts burn debuff on the enemy |
| 10 | 마법화살 | Magic Arrow |
| 10 | 적에게 강한 화살을 발사한다 | Fires a powerful arrow at the enemy |
| 11 | 폭발의서 | Scroll of Explosion |
| 11 | 적들에게 강력한 폭발을 일으킨다 | Creates a powerful explosion against enemies |

- [ ] **Step 1: Translate EquipmentData.csv**

Replace all Korean names and descriptions with English equivalents per the translation map above.

- [ ] **Step 2: Translate MonsterData.csv**

Replace Korean monster names and elements with English equivalents.

- [ ] **Step 3: Translate MonsterSkill.csv**

Replace Korean skill names and descriptions with English equivalents.

- [ ] **Step 4: Translate SkillData.csv**

Replace Korean skill names and descriptions with English equivalents.

- [ ] **Step 5: Translate ConsumableData.csv**

Replace Korean consumable names and descriptions with English equivalents.

- [ ] **Step 6: Commit CSV changes**

```bash
git add Assets/EditorData/Data/csv/*.csv
git commit -m "translate: convert all CSV data files from Korean to English"
```

---

### Task 2: Translate GameData.xlsm

**Files:**
- Modify: `Assets/EditorData/Data/GameData.xlsm`

- [ ] **Step 1: Translate xlsm sheets**

Open GameData.xlsm and translate all Korean text in each sheet to match the CSV translations. Each sheet corresponds to a CSV file.

- [ ] **Step 2: Commit xlsm changes**

```bash
git add Assets/EditorData/Data/GameData.xlsm
git commit -m "translate: convert GameData.xlsm from Korean to English"
```

---

### Task 3: Translate C# Code Files

**Files to modify:**

| File | Korean Content |
|------|----------------|
| `Simulator/BattleSimulator.cs` | Monster names, scenario names, console output |
| `Assets/Scripts/Combat/BattleManager.cs` | Battle log messages |
| `Assets/Scripts/UI/Battle/BattleUI.cs` | Victory/Defeat/Game Over text |
| `Assets/Scripts/Character/BattleEntity.cs` | Debug log messages |
| `Assets/Scripts/Character/Player.cs` | Debug log messages |
| `Assets/Scripts/UI/Inventory/ItemTooltipUI.cs` | Tooltip text, effect descriptions |
| `Assets/Scripts/Dungeon/UI/ShopUI.cs` | Debug log messages |
| `Assets/Scripts/Editor/CSVConverter.cs` | Debug logs, element/buff type mapping |
| `Assets/Scripts/Editor/ScriptableObjectAddressablesSetter.cs` | Debug log messages |
| `Assets/Scripts/Core/GameDataManager.cs` | Debug log messages |
| `Assets/Scripts/ScriptableObjects/PlayerDataSO.cs` | Header attributes |
| `Assets/Scripts/Core/WindowAspectRatio.cs` | Comments |

#### BattleSimulator.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 11 | === GreedDungeon 다전투 시뮬레이터 === | === GreedDungeon Multi-Battle Simulator === |
| 12 | 설정: 난이도 1% 증가, HP 70% 회복 | Settings: Difficulty +1% per battle, HP 70% recovery |
| 16 | 슬라임 | Slime |
| 17 | 분열 | Fission |
| 18 | 재생 | Regeneration |
| 19 | 역병쥐 | Plague Rat |
| 20 | 역병 | Plague |
| 21 | 연속공격 | Double Strike |
| 22 | 거미 | Spider |
| 23 | 독침 | Venom Sting |
| 24 | 독가스 | Poison Gas |
| 25 | 해골 | Skeleton |
| 26 | 강력한일격 | Devastating Blow |
| 27 | 분노 | Rage |
| 28 | 켈베로스 | Cerberus |
| 29 | 지옥불 | Hellfire |
| 30 | 강타 | Power Strike |
| 35 | 시작 장비 + 스킬 | Starting Equipment + Skill |
| 36 | 장비 1개 | 1 Equipment |
| 37 | 장비 2개 | 2 Equipment |
| 38 | 장비 3개 | 3 Equipment |
| 39 | 장비 4개 | 4 Equipment |
| 60 | 전투 생존 | Battles Survived |
| 68 | 평균 생존 전투 수 | Average Battles Survived |
| 71 | 완료! | Complete! |
| 93 | 켈베로스 | Cerberus |
| 149 | 켈베로스 | Cerberus |

#### BattleManager.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 105 | 전투: | Battle: |
| 135 | 턴 | turns |
| 141 | 종료 | ended |
| 146 | (buff log format) | Keep as-is (uses enum names) |
| 151 | 버프 종료 | buff ended |
| 178 | 공격 | Attack |
| 204 | 회 | hits |
| 226 | 방어 | Defend |
| 249 | 디버프 해제 | Debuffs removed |
| 298 | 턴 | turns |
| 470 | 전투 후 50% HP 회복 | Restore 50% HP after battle |
| 477 | 승리! | Victory! |
| 482 | 패배... | Defeat... |

#### BattleUI.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 148 | 승리! {goldEarned}G 획득 | Victory! Earned {goldEarned}G |
| 149 | 패배... | Defeat... |
| 155 | 게임 오버! | Game Over! |

#### BattleEntity.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 214 | 방어 태세! 받는 데미지 50% 감소 | Defending! Damage taken reduced by 50% |
| 242 | 디버프 {count}개 해제 | {count} debuff(s) removed |

#### Player.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 195 | 장비 추가 | Equipment added |
| 502 | 장비 추가(최고레어도) | Equipment added (highest rarity) |

#### ItemTooltipUI.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 87 | 구매/판매 | Buy/Sell |
| 142 | 효과: | Effect: |
| 143 | 수량: | Quantity: |
| 152 | HP {value} 회복 | Restores {value} HP |
| 153 | 디버프 해제 | Removes debuffs |
| 154 | {type} +{value}% ({duration}턴) | {type} +{value}% ({duration} turns) |
| 155 | 독 부여 ({duration}턴) | Inflicts poison ({duration} turns) |
| 156 | 화상 부여 ({duration}턴) | Inflicts burn ({duration} turns) |
| 157 | {value} 데미지 | {value} damage |
| 158 | 알 수 없음 | Unknown |

#### ShopUI.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 295 | 골드가 부족합니다. | Not enough gold. |
| 301 | 인벤토리가 가득 찼습니다. | Inventory is full. |

#### CSVConverter.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 60 | 에셋 이름 변경: | Asset renamed: |
| 64 | 이름 변경 실패: | Rename failed: |
| 85 | CSV 변환 완료! 총 {total}개 | CSV conversion complete! Total: {total} |
| 94 | 파일 없음: | File not found: |
| 140 | StatusEffect 변환 완료: {count}개 | StatusEffect conversion complete: {count} |
| 196 | Rarity 변환 완료: {count}개 | Rarity conversion complete: {count} |
| 221 | Skill 라인 {i} 건너뜀: 필드 수= | Skipping Skill line {i}: field count= |
| 264 | Skill 변환 완료: {count}개 | Skill conversion complete: {count} |
| 328 | Equipment 변환 완료: {count}개 | Equipment conversion complete: {count} |
| 395 | Monster 변환 완료: {count}개 | Monster conversion complete: {count} |
| 455 | Consumable 변환 완료: {count}개 | Consumable conversion complete: {count} |
| 501 | 불 | Fire |
| 502 | 물 | Water |
| 503 | 풀 | Grass |
| 596 | 힘의 | Strength |
| 597 | 철의 | Iron |
| 668 | MonsterSkill 변환 완료: {count}개 | MonsterSkill conversion complete: {count} |
| 712 | PlayerData.csv에 데이터가 없습니다. | No data in PlayerData.csv. |
| 719 | PlayerData 필드 수 부족: | PlayerData field count insufficient: |
| 748 | PlayerData 변환 완료 | PlayerData conversion complete |

#### ScriptableObjectAddressablesSetter.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 63 | ScriptableObject 라벨 설정 완료! 총 {count}개 | ScriptableObject labels set! Total: {count} |
| 114 | 스킬 아이콘 Sprite 등록 완료! 총 {count}개 | Skill icon sprites registered! Total: {count} |
| 122 | 모든 Addressables 설정 완료! | All Addressables configured! |
| 128 | Addressables 자동 설정 시작 | Starting Addressables auto-configuration |
| 136 | 주소 설정 완료. 빌드 시작... | Addresses set. Starting build... |
| 140 | 모든 작업 완료! 게임 실행 가능 | All tasks complete! Game is ready to run |
| 148 | Addressable Settings 없음! | No Addressable Settings found! |
| 155 | Addressables 빌드 완료! | Addressables build complete! |
| 185 | 속성 아이콘 Sprite 등록 완료! 총 {count}개 | Element icon sprites registered! Total: {count} |
| 233 | 주소: / 라벨: | Address: / Labels: |
| 237 | Addressable 아님 | Not Addressable |

#### GameDataManager.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 38 | 초기화 완료 - 몬스터: / 스킬: | Initialized - Monsters: / Skills: |

#### PlayerDataSO.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 9 | 기본 스탯 | Base Stats |
| 17 | 레벨업 보너스 | Level Up Bonus |
| 18 | 레벨당 기본 스탯에 더해지는 비율 (0.1 = 10%) | Ratio added to base stats per level (0.1 = 10%) |
| 21 | 시작 자원 | Starting Resources |
| 24 | 인벤토리 | Inventory |

#### WindowAspectRatio.cs Translation Map

| Line | Korean | English |
|------|--------|---------|
| 96 | 최소 크기 체크 | Minimum size check |

- [ ] **Step 1: Translate BattleSimulator.cs**

Replace all Korean strings with English equivalents.

- [ ] **Step 2: Translate BattleManager.cs**

Replace all Korean battle log strings with English equivalents.

- [ ] **Step 3: Translate BattleUI.cs**

Replace victory/defeat/game over text with English.

- [ ] **Step 4: Translate BattleEntity.cs**

Replace debug log messages with English.

- [ ] **Step 5: Translate Player.cs**

Replace debug log messages with English.

- [ ] **Step 6: Translate ItemTooltipUI.cs**

Replace tooltip text and effect descriptions with English.

- [ ] **Step 7: Translate ShopUI.cs**

Replace debug log messages with English.

- [ ] **Step 8: Translate CSVConverter.cs**

Replace debug logs, element mapping, and buff type mapping with English.

- [ ] **Step 9: Translate ScriptableObjectAddressablesSetter.cs**

Replace debug log messages with English.

- [ ] **Step 10: Translate GameDataManager.cs**

Replace debug log message with English.

- [ ] **Step 11: Translate PlayerDataSO.cs**

Replace Header/Tooltip attributes with English.

- [ ] **Step 12: Translate WindowAspectRatio.cs**

Replace comment with English.

- [ ] **Step 13: Commit C# changes**

```bash
git add Simulator/BattleSimulator.cs Assets/Scripts/**/*.cs
git commit -m "translate: convert all C# code text from Korean to English"
```

---

### Task 4: Identify Unity Scene/Prefab Text Objects

**Files to inspect:**
- `Assets/Scenes/Battle.unity`
- `Assets/Scenes/Title.unity`
- `Assets/Prefabs/IconSlot/Shop.prefab`
- `Assets/Prefabs/IconSlot/TreasurPopup.prefab`
- `Assets/Prefabs/IconSlot/PlayerEffectsContainer_Debuff.prefab`
- `Assets/Prefabs/IconSlot/PlayerEffectsContainer_buff.prefab`
- `Assets/Prefabs/IconSlot/StatusEffectSlot.prefab`
- `Assets/Prefabs/IconSlot/InventorySlotUI.prefab`

- [ ] **Step 1: Search for TextMeshPro objects with Korean text**

Note: grep already confirmed no Korean text in .unity/.prefab files. Verify by checking TextMeshPro text fields for any hardcoded Korean strings.

- [ ] **Step 2: Create a manual update list**

Document any TextMeshPro objects that need manual editing in the Unity Editor. Provide exact GameObject paths and current text values.

- [ ] **Step 3: Commit findings**

```bash
git add -A
git commit -m "docs: add translation implementation plan"
```

---

### Task 5: Post-Translation Verification

- [ ] **Step 1: Verify no remaining Korean text**

Run grep (or rg on Windows) to confirm no Korean characters remain in .cs and .csv files:

```bash
rg '[가-힣]' Assets/Scripts/ --type cs
rg '[가-힣]' Assets/EditorData/Data/csv/ --type csv
rg '[가-힣]' Simulator/ --type cs
```

Expected: No matches (or only in intentionally preserved areas).

- [ ] **Step 2: Verify CSVConverter element mapping**

Ensure the ParseElement switch in CSVConverter.cs correctly maps English element names:
- "Fire" → Element.Fire
- "Water" → Element.Water
- "Grass" → Element.Grass

- [ ] **Step 3: Verify CSVConverter buff type mapping**

Ensure ParseBuffType in CSVConverter.cs correctly maps English consumable names:
- "Potion of Strength" → BuffType.Attack
- "Potion of Iron" → BuffType.Defense

- [ ] **Step 4: Commit verification**

```bash
git add -A
git commit -m "translate: verify and fix remaining Korean text references"
```

---

### Task 6: Unity Rebuild Instructions (Manual)

These steps must be performed in the Unity Editor after all file translations are complete:

1. Open Unity Editor
2. Run `Tools > CSV > Convert All` to regenerate ScriptableObjects from translated CSVs
3. Run `Tools > Addressables > Set All SO Labels` to update labels
4. Run `Tools > Addressables > Build Addressables` to rebuild content
5. Test in Editor to verify all text displays correctly in English
6. Build WebGL target and verify no font issues
