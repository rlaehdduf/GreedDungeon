# 몬스터 특수 스킬 시스템 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 몬스터가 전용 스킬 1개와 공용 스킬 1개를 보유하여 랜덤 확률로 사용하는 시스템 구현

**Architecture:** MonsterSkillDataSO 신규 생성, MonsterDataSO에 스킬 ID 참조 추가, GameDataManager에서 Addressables로 로드, BattleManager에서 스킬 실행 로직 구현

**Tech Stack:** Unity 2D, C#, ScriptableObject, Addressables, CSV 워크플로우

---

## 파일 구조

### 신규 생성
- `Assets/Scripts/ScriptableObjects/MonsterSkillDataSO.cs` - 몬스터 스킬 데이터
- `Assets/EditorData/Data/csv/MonsterSkill.csv` - 스킬 CSV 데이터

### 수정
- `Assets/Scripts/ScriptableObjects/MonsterDataSO.cs` - 스킬 ID 필드 추가
- `Assets/Scripts/Core/GameDataManager.cs` - 스킬 로드/조회 메서드
- `Assets/Scripts/Character/Monster.cs` - 스킬 보유/선택 로직
- `Assets/Scripts/Combat/BattleManager.cs` - 스킬 실행 로직
- `Assets/Scripts/Combat/BattleController.cs` - 스킬 주입
- `Assets/Scripts/Editor/CSVConverter.cs` - 스킬 변환 로직
- `Assets/EditorData/Data/csv/MonsterData.csv` - 몬스터 데이터 수정

---

## Task 1: MonsterSkillDataSO 생성

**Files:**
- Create: `Assets/Scripts/ScriptableObjects/MonsterSkillDataSO.cs`

- [ ] **Step 1: MonsterSkillDataSO.cs 작성**

```csharp
using GreedDungeon.Core;
using UnityEngine;

namespace GreedDungeon.ScriptableObjects
{
    public enum MonsterSkillType
    {
        Attack,
        Buff,
        Debuff,
        Heal
    }

    [CreateAssetMenu(fileName = "MonsterSkillData", menuName = "GreedDungeon/Data/MonsterSkill")]
    public class MonsterSkillDataSO : ScriptableObject, IData
    {
        public int ID;
        public string Name;
        [TextArea] public string Description;
        public MonsterSkillType SkillType;
        public bool IsShared;
        
        [Header("Attack")]
        public float DamageMultiplier = 1f;
        public int HitCount = 1;
        
        [Header("Debuff")]
        public string StatusEffectID;
        [Range(0f, 100f)] public float StatusEffectChance;
        
        [Header("Buff")]
        public BuffType BuffType;
        public float BuffValue;
        public int BuffDuration;
        
        [Header("Heal")]
        public float HealPercent;
        
        int IData.ID => ID;
    }
}
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/ScriptableObjects/MonsterSkillDataSO.cs
git commit -m "feat: MonsterSkillDataSO 및 MonsterSkillType enum 추가"
```

---

## Task 2: MonsterDataSO 수정

**Files:**
- Modify: `Assets/Scripts/ScriptableObjects/MonsterDataSO.cs`

- [ ] **Step 1: 필드 수정**

기존 코드 (line 29):
```csharp
public string SpecialSkill;
```

새 코드로 변경:
```csharp
public int UniqueSkillID;
public int SharedSkillID;
[Range(0f, 100f)] public float SkillChance;
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/ScriptableObjects/MonsterDataSO.cs
git commit -m "feat: MonsterDataSO에 UniqueSkillID, SharedSkillID, SkillChance 추가"
```

---

## Task 3: CSVConverter 수정

**Files:**
- Modify: `Assets/Scripts/Editor/CSVConverter.cs`

- [ ] **Step 1: GetAssetID에 MonsterSkillDataSO 추가**

```csharp
private static int GetAssetID(UnityEngine.Object asset)
{
    return asset switch
    {
        StatusEffectDataSO s => s.ID,
        RarityDataSO r => r.ID,
        SkillDataSO sk => sk.ID,
        EquipmentDataSO e => e.ID,
        MonsterDataSO m => m.ID,
        ConsumableDataSO c => c.ID,
        MonsterSkillDataSO ms => ms.ID,
        _ => -1
    };
}
```

- [ ] **Step 2: ConvertAll에 ConvertMonsterSkills 추가**

`ConvertAll()` 메서드 내 `total += ConvertConsumables();` 다음 줄에 추가:

```csharp
public static void ConvertAll()
{
    int total = 0;
    total += ConvertStatusEffects();
    total += ConvertRarities();
    total += ConvertSkills();
    total += ConvertEquipments();
    total += ConvertMonsters();
    total += ConvertConsumables();
    total += ConvertMonsterSkills();  // 추가
    
    // ...
}
```

- [ ] **Step 3: ConvertMonsterSkills 메서드 추가**

파일 끝에 추가:

```csharp
[MenuItem("Tools/CSV/Convert MonsterSkills")]
public static int ConvertMonsterSkills()
{
    string csvFile = Path.Combine(CSV_PATH, "MonsterSkill.csv");
    if (!File.Exists(csvFile))
    {
        Debug.LogWarning($"파일 없음: {csvFile}");
        return 0;
    }

    string outputPath = Path.Combine(OUTPUT_PATH, "MonsterSkills");
    if (!Directory.Exists(outputPath))
        Directory.CreateDirectory(outputPath);

    var lines = ReadCSV(csvFile);
    int count = 0;
    for (int i = 1; i < lines.Count; i++)
    {
        var values = lines[i];
        if (values.Count < 13 || !int.TryParse(values[0], out int id)) continue;

        var data = FindExistingAsset<MonsterSkillDataSO>(id, outputPath);
        bool isNew = data == null;
        
        if (isNew)
        {
            data = ScriptableObject.CreateInstance<MonsterSkillDataSO>();
        }
        else
        {
            RenameAssetIfNeeded(data, id, values[1], outputPath);
        }

        data.ID = id;
        data.Name = values[1];
        data.Description = values[2];
        data.SkillType = ParseMonsterSkillType(values[3]);
        data.IsShared = values[4].ToLower() == "true";
        data.DamageMultiplier = float.TryParse(values[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float dm) ? dm : 1f;
        data.HitCount = int.TryParse(values[6], out int hc) ? hc : 1;
        data.StatusEffectID = values[7] == "None" ? "" : values[7];
        data.StatusEffectChance = float.TryParse(values[8], NumberStyles.Float, CultureInfo.InvariantCulture, out float sec) ? sec : 0;
        data.BuffType = ParseBuffTypeForMonsterSkill(values[9]);
        data.BuffValue = float.TryParse(values[10], NumberStyles.Float, CultureInfo.InvariantCulture, out float bv) ? bv : 0;
        data.BuffDuration = int.TryParse(values[11], out int bd) ? bd : 0;
        data.HealPercent = float.TryParse(values[12], NumberStyles.Float, CultureInfo.InvariantCulture, out float hp) ? hp : 0;

        if (isNew)
        {
            string assetPath = Path.Combine(outputPath, GetAssetFileName<MonsterSkillDataSO>(id, data.Name));
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        EditorUtility.SetDirty(data);
        count++;
    }
    
    Debug.Log($"MonsterSkill 변환 완료: {count}개");
    return count;
}

private static MonsterSkillType ParseMonsterSkillType(string value)
{
    return value switch
    {
        "Attack" => MonsterSkillType.Attack,
        "Buff" => MonsterSkillType.Buff,
        "Debuff" => MonsterSkillType.Debuff,
        "Heal" => MonsterSkillType.Heal,
        _ => MonsterSkillType.Attack
    };
}

private static BuffType ParseBuffTypeForMonsterSkill(string value)
{
    return value switch
    {
        "Attack" => BuffType.Attack,
        "Defense" => BuffType.Defense,
        "Speed" => BuffType.Speed,
        _ => BuffType.None
    };
}
```

- [ ] **Step 4: ConvertMonsters 수정**

`ConvertMonsters()` 메서드 수정:

1. 필드 개수 체크 변경:
```csharp
// 기존: if (values.Count < 17 || !int.TryParse(values[0], out int id)) continue;
if (values.Count < 19 || !int.TryParse(values[0], out int id)) continue;
```

2. 기존 `data.SpecialSkill = values[12];` 라인을 다음으로 교체:
```csharp
data.UniqueSkillID = int.TryParse(values[12], out int usid) ? usid : 0;
data.SharedSkillID = int.TryParse(values[13], out int ssid) ? ssid : 0;
data.SkillChance = float.TryParse(values[14], NumberStyles.Float, CultureInfo.InvariantCulture, out float sc) ? sc : 0;
```

3. 이후 인덱스 수정:
```csharp
data.IsBoss = values[15].ToLower().Replace("fasle", "false") == "true";
data.PrefabAddress = values[16];
data.ScaleX = float.TryParse(values[17], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) ? sx : 1f;
data.ScaleY = float.TryParse(values[18], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy) ? sy : 1f;
```

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Editor/CSVConverter.cs
git commit -m "feat: CSVConverter에 MonsterSkill 변환 로직 추가"
```

---

## Task 4: IGameDataManager 인터페이스 수정

**Files:**
- Modify: `Assets/Scripts/Core/GameDataManager.cs`

- [ ] **Step 1: 인터페이스에 메서드 추가**

```csharp
MonsterSkillDataSO GetMonsterSkillData(int id);
IReadOnlyList<MonsterSkillDataSO> GetAllMonsterSkillData();
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/Core/GameDataManager.cs
git commit -m "feat: IGameDataManager에 MonsterSkill 조회 메서드 추가"
```

---

## Task 5: GameDataManager 구현 수정

**Files:**
- Modify: `Assets/Scripts/Core/GameDataManager.cs`

- [ ] **Step 1: 필드 추가**

```csharp
private readonly Dictionary<int, MonsterSkillDataSO> _monsterSkills = new();
private readonly List<MonsterSkillDataSO> _monsterSkillList = new();
```

- [ ] **Step 2: LoadAllDataAsync에 스킬 로드 추가**

```csharp
await Task.WhenAll(
    LoadDataAsync("MonsterData", _monsters, _monsterList),
    LoadDataAsync("SkillData", _skills, _skillList),
    LoadDataAsync("EquipmentData", _equipments, _equipmentList),
    LoadDataAsync("ConsumableData", _consumables, _consumableList),
    LoadDataAsync("RarityData", _rarities, _rarityList),
    LoadDataAsync("StatusEffectData", _statusEffects),
    LoadDataAsync("MonsterSkillData", _monsterSkills, _monsterSkillList)
);
```

- [ ] **Step 3: 조회 메서드 구현**

```csharp
public MonsterSkillDataSO GetMonsterSkillData(int id) 
    => _monsterSkills.TryGetValue(id, out var data) ? data : null;

public IReadOnlyList<MonsterSkillDataSO> GetAllMonsterSkillData() 
    => _monsterSkillList;
```

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Core/GameDataManager.cs
git commit -m "feat: GameDataManager에 MonsterSkill 로드/조회 구현"
```

---

## Task 6: Monster 클래스 수정

**Files:**
- Modify: `Assets/Scripts/Character/Monster.cs`

- [ ] **Step 1: 스킬 필드 및 프로퍼티 추가**

```csharp
private MonsterSkillDataSO _uniqueSkill;
private MonsterSkillDataSO _sharedSkill;

public MonsterSkillDataSO UniqueSkill => _uniqueSkill;
public MonsterSkillDataSO SharedSkill => _sharedSkill;
public float SkillChance => _data.SkillChance;
```

- [ ] **Step 2: SetSkills 메서드 추가**

```csharp
public void SetSkills(MonsterSkillDataSO unique, MonsterSkillDataSO shared)
{
    _uniqueSkill = unique;
    _sharedSkill = shared;
}
```

- [ ] **Step 3: GetRandomSkill 메서드 추가**

```csharp
public MonsterSkillDataSO GetRandomSkill()
{
    if (UnityEngine.Random.value * 100 >= _data.SkillChance)
        return null;
    
    var skills = new List<MonsterSkillDataSO>();
    if (_uniqueSkill != null) skills.Add(_uniqueSkill);
    if (_sharedSkill != null) skills.Add(_sharedSkill);
    
    return skills.Count > 0 
        ? skills[UnityEngine.Random.Range(0, skills.Count)] 
        : null;
}
```

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Character/Monster.cs
git commit -m "feat: Monster에 스킬 보유/선택 로직 추가"
```

---

## Task 7: BattleManager 수정 (인터페이스 + 구현)

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`

- [ ] **Step 1: 이벤트 추가 (IBattleManager 인터페이스)**

인터페이스에 이벤트 추가:
```csharp
event Action<int, int> OnMonsterHealed;
event Action<BuffType, float, int> OnMonsterBuffApplied;
```

- [ ] **Step 2: 이벤트 구현 (BattleManager 클래스)**

```csharp
public event Action<int, int> OnMonsterHealed;
public event Action<BuffType, float, int> OnMonsterBuffApplied;
```

- [ ] **Step 3: ExecuteMonsterSkill 메서드 시그니처 추가 (IBattleManager)**

```csharp
void ExecuteMonsterSkill(MonsterSkillDataSO skill);
```

- [ ] **Step 4: ExecuteMonsterSkill 구현 (BattleManager 클래스)**

```csharp
public void ExecuteMonsterSkill(MonsterSkillDataSO skill)
{
    switch (skill.SkillType)
    {
        case MonsterSkillType.Attack:
            ExecuteMonsterAttackSkill(skill);
            break;
        case MonsterSkillType.Buff:
            ExecuteMonsterBuffSkill(skill);
            break;
        case MonsterSkillType.Debuff:
            ExecuteMonsterDebuffSkill(skill);
            break;
        case MonsterSkillType.Heal:
            ExecuteMonsterHealSkill(skill);
            break;
    }
}
```

- [ ] **Step 5: ExecuteMonsterAttackSkill 구현**

```csharp
private void ExecuteMonsterAttackSkill(MonsterSkillDataSO skill)
{
    int baseDamage = _monster.TotalStats.Attack;
    int totalDamage = 0;
    
    for (int i = 0; i < skill.HitCount; i++)
    {
        int damage = Mathf.RoundToInt(baseDamage * skill.DamageMultiplier);
        if (_player.IsDefending) damage /= 2;
        damage = Mathf.Max(1, damage - _player.TotalStats.Defense / 2);
        _player.TakeDamage(damage);
        totalDamage += damage;
    }
    
    OnPlayerDamaged?.Invoke(totalDamage, false);
    LogBattle($"{_monster.Name} {skill.Name}! → {totalDamage} Dmg", UI.Battle.LogType.Monster);
}
```

- [ ] **Step 6: ExecuteMonsterBuffSkill 구현**

```csharp
private void ExecuteMonsterBuffSkill(MonsterSkillDataSO skill)
{
    if (skill.BuffType == BuffType.None) return;
    
    _monster.ApplyBuff(skill.BuffType, skill.BuffValue, skill.BuffDuration);
    OnMonsterBuffApplied?.Invoke(skill.BuffType, skill.BuffValue, skill.BuffDuration);
    LogBattle($"{_monster.Name} {skill.Name}! {skill.BuffType} +{skill.BuffValue}%", UI.Battle.LogType.Monster);
}
```

- [ ] **Step 7: ExecuteMonsterDebuffSkill 구현**

```csharp
private void ExecuteMonsterDebuffSkill(MonsterSkillDataSO skill)
{
    int damage = Mathf.RoundToInt(_monster.TotalStats.Attack * skill.DamageMultiplier);
    if (_player.IsDefending) damage /= 2;
    damage = Mathf.Max(1, damage - _player.TotalStats.Defense / 2);
    
    _player.TakeDamage(damage);
    OnPlayerDamaged?.Invoke(damage, false);
    LogBattle($"{_monster.Name} {skill.Name}! → {damage} Dmg", UI.Battle.LogType.Monster);
    
    if (!string.IsNullOrEmpty(skill.StatusEffectID) && 
        UnityEngine.Random.value * 100 < skill.StatusEffectChance)
    {
        var effect = FindStatusEffect(skill.StatusEffectID);
        if (effect != null)
        {
            _player.ApplyStatusEffect(effect, effect.Duration);
            LogBattle($"→ {effect.Name}", UI.Battle.LogType.Monster);
        }
    }
}
```

- [ ] **Step 8: ExecuteMonsterHealSkill 구현**

```csharp
private void ExecuteMonsterHealSkill(MonsterSkillDataSO skill)
{
    int healAmount = Mathf.RoundToInt(_monster.BaseStats.MaxHP * skill.HealPercent / 100f);
    _monster.Heal(healAmount);
    OnMonsterHealed?.Invoke(healAmount, _monster.CurrentHP);
    LogBattle($"{_monster.Name} {skill.Name}! HP +{healAmount}", UI.Battle.LogType.Monster);
}
```

- [ ] **Step 9: 커밋**

```bash
git add Assets/Scripts/Combat/BattleManager.cs
git commit -m "feat: BattleManager에 몬스터 스킬 이벤트 및 실행 로직 구현"
```

---

## Task 8: BattleController 수정

**Files:**
- Modify: `Assets/Scripts/Combat/BattleController.cs`

- [ ] **Step 1: StartTestBattle에 스킬 주입 로직 추가 (line 307 이후)**

`_currentMonster = new Monster(monsterData);` (line 306) 다음에 추가:

```csharp
MonsterSkillDataSO uniqueSkill = null;
MonsterSkillDataSO sharedSkill = null;

if (monsterData.UniqueSkillID > 0)
    uniqueSkill = _gameDataManager.GetMonsterSkillData(monsterData.UniqueSkillID);
if (monsterData.SharedSkillID > 0)
    sharedSkill = _gameDataManager.GetMonsterSkillData(monsterData.SharedSkillID);

_currentMonster.SetSkills(uniqueSkill, sharedSkill);
```

- [ ] **Step 2: HandleMonsterTurnCoroutine에 스킬 사용 분기 추가 (line 208 이후)**

`_battleManager.ExecuteMonsterAttack();` 호출 부분을 다음으로 교체:

```csharp
var skill = _currentMonster.GetRandomSkill();

if (skill != null)
{
    _battleManager.ExecuteMonsterSkill(skill);
}
else
{
    _battleManager.ExecuteMonsterAttack();
}
```

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: BattleController에 몬스터 스킬 주입 및 사용 분기 추가"
```

---

## Task 9: CSV 데이터 파일 생성

**Files:**
- Create: `Assets/EditorData/Data/csv/MonsterSkill.csv`
- Modify: `Assets/EditorData/Data/csv/MonsterData.csv`

- [ ] **Step 1: MonsterSkill.csv 생성**

```csv
ID,Name,Description,SkillType,IsShared,DamageMultiplier,HitCount,StatusEffectID,StatusEffectChance,BuffType,BuffValue,BuffDuration,HealPercent
1,강타,강력한 일격을 가합니다,Attack,true,1.5,1,None,0,None,0,0,0
2,연속공격,빠른 속도로 2회 공격합니다,Attack,true,1,2,None,0,None,0,0,0
3,독가스,독성 가스로 중독시킵니다,Debuff,true,1,1,2,30,None,0,0,0
4,분노,공격력이 증가합니다,Buff,true,0,0,None,0,Attack,30,3,0
5,재생,체력을 회복합니다,Heal,true,0,0,None,0,None,0,0,20
101,분열,몸을 분열시켜 회복합니다,Heal,false,0,0,None,0,None,0,0,30
102,약점공격,약점을 노려 강하게 공격합니다,Attack,false,2,1,None,0,None,0,0,0
103,전투함성,함성으로 공격력이 증가합니다,Buff,false,0,0,None,0,Attack,50,2,0
104,화염숨,화염을 내뿜습니다,Debuff,false,1,2,1,50,None,0,0,0
```

> **중요:** 위 예시 스킬들은 추후 밸런스 조정을 위해 수정이 필요합니다.

- [ ] **Step 2: MonsterData.csv 수정**

기존 헤더:
```
ID,Name,Element,MaxHP,Attack,Defense,Speed,CriticalRate,GoldDropMin,GoldDropMax,StatusEffectID,StatusEffectChance,SpecialSkill,IsBoss,PrefabAddress,ScaleX,ScaleY
```

새 헤더 (`SpecialSkill` 제거, 3개 컬럼 추가):
```
ID,Name,Element,MaxHP,Attack,Defense,Speed,CriticalRate,GoldDropMin,GoldDropMax,StatusEffectID,StatusEffectChance,UniqueSkillID,SharedSkillID,SkillChance,IsBoss,PrefabAddress,ScaleX,ScaleY
```

기존 데이터 예시:
```
1,슬라임,풀,50,10,5,10,0,10,20,None,0,None,false,Monsters/Slime,1,1
```

새 데이터 예시 (슬라임에 전용 스킬 101 적용, 30% 발동 확률):
```
1,슬라임,풀,50,10,5,10,0,10,20,None,0,101,0,30,false,Monsters/Slime,1,1
```

다른 몬스터들은 스킬 없음:
```
2,고블린,불,80,15,8,12,5,20,40,None,0,0,0,0,false,Monsters/Goblin,1,1
```

- [ ] **Step 3: 커밋**

```bash
git add Assets/EditorData/Data/csv/MonsterSkill.csv
git add Assets/EditorData/Data/csv/MonsterData.csv
git commit -m "feat: MonsterSkill.csv 생성 및 MonsterData.csv 수정"
```

---

## Task 10: Unity 수동 설정

**수동 작업 필요**

- [ ] **Step 1: MonsterSkills 폴더 생성**
- Unity에서 `Assets/ScriptableObjects/Data/MonsterSkills` 폴더 생성

- [ ] **Step 2: CSV 변환 실행**
- Unity 메뉴: `Tools → CSV → Convert All`

- [ ] **Step 3: Addressables 레이블 설정**
- `Assets/ScriptableObjects/Data/MonsterSkills` 폴더의 모든 MonsterSkillDataSO 에셋 선택
- Addressables Groups에서 "MonsterSkillData" 레이블 추가

- [ ] **Step 4: Addressables 빌드**
- Unity 메뉴: `Tools → Addressables → 🔄 Setup & Build`

---

## Task 11: 테스트 및 검증

- [ ] **Step 1: Unity 플레이 테스트**
- 게임 실행 후 몬스터가 스킬을 사용하는지 확인
- 전투 로그에 스킬 이름이 표시되는지 확인

- [ ] **Step 2: 로그 확인**
- Console에서 `[GameDataManager] 초기화 완료` 메시지 확인
- MonsterSkill 데이터가 로드되었는지 확인

---

## 참고사항

- **예시 스킬 데이터는 밸런스 조정이 필요합니다** (Task 10의 CSV 데이터)
- 기존 몬스터 데이터는 스킬이 없으므로 `UniqueSkillID=0, SharedSkillID=0, SkillChance=0`으로 설정
- 새 몬스터 추가 시 CSV에서 스킬 ID와 발동 확률 설정 필요