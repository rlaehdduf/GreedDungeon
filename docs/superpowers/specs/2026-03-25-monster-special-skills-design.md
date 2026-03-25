# 몬스터 특수 스킬 시스템 설계

## 개요

몬스터가 전용 스킬 1개와 공용 스킬 1개를 보유하여 전투의 다양성을 높이는 시스템.

## 목표

- 몬스터마다 고유한 특수 능력 부여
- 공용 스킬로 스킬 데이터 재사용성 확보
- 랜덤 확률 기반 스킬 발동

## 데이터 구조

### MonsterSkillDataSO (신규 ScriptableObject)

```
파일: Assets/Scripts/ScriptableObjects/MonsterSkillDataSO.cs

필드:
- ID: int                       # 스킬 식별자
- Name: string                  # 스킬 이름
- Description: string           # 설명
- SkillType: MonsterSkillType   # Attack, Buff, Debuff, Heal
- IsShared: bool                # 공용 스킬 여부

공격 관련:
- DamageMultiplier: float       # 데미지 배율 (1.0 = 100%, 1.5 = 150%)
- HitCount: int                 # 타격 횟수

디버프 관련:
- StatusEffectID: string        # 적용할 상태이상 ID
- StatusEffectChance: float     # 적용 확률 (0~100)

버프 관련:
- BuffType: BuffType            # 버프 타입 (Attack, Defense, Speed)
- BuffValue: float              # 버프 수치 (30 = 30% 증가)
- BuffDuration: int             # 지속 턴수

회복 관련:
- HealPercent: float            # 최대 HP 대비 회복 비율 (20 = 20%)
```

### MonsterSkillType Enum

```
enum MonsterSkillType
{
    Attack,   # 공격 스킬
    Buff,     # 자신에게 버프
    Debuff,   # 플레이어에게 디버프
    Heal      # 자신 회복
}
```

### MonsterDataSO (수정)

```
제거 필드:
- SpecialSkill: string   # 기존 필드 제거

추가 필드:
- UniqueSkillID: int       # 전용 스킬 ID (0 = 없음)
- SharedSkillID: int       # 공용 스킬 ID (0 = 없음)  
- SkillChance: float       # 스킬 발동 확률 (0~100)
```

### CSV 수정

**MonsterData.csv 수정:**
```
기존 SpecialSkill 컬럼 제거
추가 컬럼: UniqueSkillID, SharedSkillID, SkillChance
```

**신규 MonsterSkill.csv:**
```
ID,Name,Description,SkillType,IsShared,DamageMultiplier,HitCount,StatusEffectID,StatusEffectChance,BuffType,BuffValue,BuffDuration,HealPercent
```

## 클래스 구조

### MonsterSkillDataSO

```csharp
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
    
    public float DamageMultiplier = 1f;
    public int HitCount = 1;
    public string StatusEffectID;
    [Range(0f, 100f)] public float StatusEffectChance;
    public BuffType BuffType;
    public float BuffValue;
    public int BuffDuration;
    public float HealPercent;
    
    int IData.ID => ID;
}
```

### IGameDataManager (수정)

```csharp
public interface IGameDataManager
{
    // 기존 메서드...
    
    MonsterSkillDataSO GetMonsterSkillData(int id);
    List<MonsterSkillDataSO> GetAllMonsterSkillData();
}
```

### GameDataManager (수정)

```csharp
private Dictionary<int, MonsterSkillDataSO> _monsterSkillData;

public MonsterSkillDataSO GetMonsterSkillData(int id)
{
    if (_monsterSkillData == null || !_monsterSkillData.TryGetValue(id, out var data))
        return null;
    return data;
}

public List<MonsterSkillDataSO> GetAllMonsterSkillData()
{
    return _monsterSkillData?.Values.ToList();
}

// InitializeAsync() 내부
private void LoadMonsterSkillData()
{
    var folder = "Assets/ScriptableObjects/Data/MonsterSkills";
    var guids = AssetDatabase.FindAssets("t:MonsterSkillDataSO", new[] { folder });
    _monsterSkillData = new Dictionary<int, MonsterSkillDataSO>();
    
    foreach (var guid in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var data = AssetDatabase.LoadAssetAtPath<MonsterSkillDataSO>(path);
        if (data != null)
            _monsterSkillData[data.ID] = data;
    }
}
```

### Monster (수정)

```csharp
public class Monster : BattleEntity
{
    private readonly MonsterDataSO _data;
    private MonsterSkillDataSO _uniqueSkill;
    private MonsterSkillDataSO _sharedSkill;
    
    public MonsterSkillDataSO UniqueSkill => _uniqueSkill;
    public MonsterSkillDataSO SharedSkill => _sharedSkill;
    public float SkillChance => _data.SkillChance;
    
    public Monster(MonsterDataSO data, StatusEffectDataSO statusEffectData = null)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _statusEffectData = statusEffectData;
        InitializeStats(...);
    }
    
    public void SetSkills(MonsterSkillDataSO unique, MonsterSkillDataSO shared)
    {
        _uniqueSkill = unique;
        _sharedSkill = shared;
    }
    
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
}
```

### IBattleManager (수정)

```csharp
public interface IBattleManager
{
    // 기존 이벤트...
    event Action<int, int> OnMonsterHealed;
    event Action<BuffType, float, int> OnMonsterBuffApplied;
}
```

### BattleManager (수정)

```csharp
public event Action<int, int> OnMonsterHealed;
public event Action<BuffType, float, int> OnMonsterBuffApplied;

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
    LogBattle($"{_monster.Name} {skill.Name}! → {totalDamage} Dmg", LogType.Monster);
}

private void ExecuteMonsterBuffSkill(MonsterSkillDataSO skill)
{
    if (skill.BuffType == BuffType.None) return;
    
    _monster.ApplyBuff(skill.BuffType, skill.BuffValue, skill.BuffDuration);
    OnMonsterBuffApplied?.Invoke(skill.BuffType, skill.BuffValue, skill.BuffDuration);
    LogBattle($"{_monster.Name} {skill.Name}! {skill.BuffType} +{skill.BuffValue}%", LogType.Monster);
}

private void ExecuteMonsterDebuffSkill(MonsterSkillDataSO skill)
{
    int damage = Mathf.RoundToInt(_monster.TotalStats.Attack * skill.DamageMultiplier);
    if (_player.IsDefending) damage /= 2;
    damage = Mathf.Max(1, damage - _player.TotalStats.Defense / 2);
    
    _player.TakeDamage(damage);
    OnPlayerDamaged?.Invoke(damage, false);
    LogBattle($"{_monster.Name} {skill.Name}! → {damage} Dmg", LogType.Monster);
    
    if (!string.IsNullOrEmpty(skill.StatusEffectID) && 
        UnityEngine.Random.value * 100 < skill.StatusEffectChance)
    {
        var effect = FindStatusEffect(skill.StatusEffectID);
        if (effect != null)
        {
            _player.ApplyStatusEffect(effect, effect.Duration);
            LogBattle($"→ {effect.Name}", LogType.Monster);
        }
    }
}

private void ExecuteMonsterHealSkill(MonsterSkillDataSO skill)
{
    int healAmount = Mathf.RoundToInt(_monster.BaseStats.MaxHP * skill.HealPercent / 100f);
    _monster.Heal(healAmount);
    OnMonsterHealed?.Invoke(healAmount, _monster.CurrentHP);
    LogBattle($"{_monster.Name} {skill.Name}! HP +{healAmount}", LogType.Monster);
}
```

### BattleController (수정)

```csharp
public void StartTestBattle()
{
    if (_gameDataManager == null) return;
    
    var monsterData = _gameDataManager.GetMonsterData(1);
    if (monsterData == null) return;
    
    _currentMonster = new Monster(monsterData);
    
    // 스킬 주입
    if (monsterData.UniqueSkillID > 0)
    {
        var uniqueSkill = _gameDataManager.GetMonsterSkillData(monsterData.UniqueSkillID);
        var sharedSkill = monsterData.SharedSkillID > 0 
            ? _gameDataManager.GetMonsterSkillData(monsterData.SharedSkillID) 
            : null;
        _currentMonster.SetSkills(uniqueSkill, sharedSkill);
    }
    
    _testPlayer = new Player();
    AddTestItemsToInventory();
    
    if (_battleUI != null)
        _battleUI.SetupBattle(_testPlayer, _currentMonster);
    
    _battleManager.StartBattle(_testPlayer, _currentMonster);
}

private IEnumerator HandleMonsterTurnCoroutine()
{
    // ... 기존 대기 로직 ...
    
    var skill = _currentMonster.GetRandomSkill();
    
    if (skill != null)
    {
        _battleManager.ExecuteMonsterSkill(skill);
    }
    else
    {
        _battleManager.ExecuteMonsterAttack();
    }
    
    // ... 후속 처리 ...
}
```

### CSVConverter (수정)

```csharp
// GetAssetID에 추가
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
        MonsterSkillDataSO ms => ms.ID,  // 추가
        _ => -1
    };
}

// ConvertAll에 추가
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
    
    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    
    Debug.Log($"═══ CSV 변환 완료! 총 {total}개 ═══");
}

// 신규 메서드
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

// ConvertMonsters 수정
public static int ConvertMonsters()
{
    // ... 기존 코드 ...
    
    // 기존 SpecialSkill 대신 새 필드 사용
    // data.SpecialSkill = values[12];  // 제거
    data.UniqueSkillID = int.TryParse(values[12], out int usid) ? usid : 0;
    data.SharedSkillID = int.TryParse(values[13], out int ssid) ? ssid : 0;
    data.SkillChance = float.TryParse(values[14], NumberStyles.Float, CultureInfo.InvariantCulture, out float sc) ? sc : 0;
    
    // 인덱스 조정
    data.IsBoss = values[15].ToLower().Replace("fasle", "false") == "true";
    data.PrefabAddress = values[16];
    data.ScaleX = float.TryParse(values[17], NumberStyles.Float, CultureInfo.InvariantCulture, out float sx) ? sx : 1f;
    data.ScaleY = float.TryParse(values[18], NumberStyles.Float, CultureInfo.InvariantCulture, out float sy) ? sy : 1f;
    
    // ...
}
```

## 실행 흐름

```
CSVConverter.ConvertAll()
    ↓ ConvertMonsterSkills()
MonsterSkillDataSO 에셋 생성
    ↓
GameDataManager.InitializeAsync()
    ↓ LoadMonsterSkillData()
_monsterSkillData Dictionary 구축
    ↓
BattleController.StartTestBattle()
    ↓ new Monster(monsterData)
    ↓ monster.SetSkills(GetMonsterSkillData(uniqueID), GetMonsterSkillData(sharedID))
Monster에 스킬 주입 완료
    ↓
HandleMonsterTurnCoroutine()
    ↓ monster.GetRandomSkill()
    ↓ SkillChance 체크
skill != null → BattleManager.ExecuteMonsterSkill(skill)
skill == null → BattleManager.ExecuteMonsterAttack()
```

## 공용 스킬 예시

| ID | 이름 | 타입 | IsShared | 효과 |
|----|------|------|----------|------|
| 1 | 강타 | Attack | true | 150% 데미지 |
| 2 | 연속공격 | Attack | true | 2회 타격 |
| 3 | 독가스 | Debuff | true | 중독 30% 확률 |
| 4 | 분노 | Buff | true | 공격력 +30% 3턴 |
| 5 | 재생 | Heal | true | HP 20% 회복 |

## 전용 스킬 예시

| ID | 이름 | 타입 | IsShared | 대상 몬스터 | 효과 |
|----|------|------|----------|------------|------|
| 101 | 분열 | Heal | false | 슬라임 | HP 30% 회복 |
| 102 | 약점공격 | Attack | false | 고블린 | 200% 데미지 |
| 103 | 전투함성 | Buff | false | 오크 | 공격력 +50% 2턴 |
| 104 | 화염숨 | Debuff | false | 드래곤 | 2회 타격 + 화상 |

## 구현 순서

1. MonsterSkillType enum 정의 (MonsterSkillDataSO.cs 내부)
2. MonsterSkillDataSO.cs 생성
3. MonsterDataSO 수정 (SpecialSkill 제거, UniqueSkillID/SharedSkillID/SkillChance 추가)
4. CSVConverter 수정 (ConvertMonsterSkills, ParseMonsterSkillType, ParseBuffTypeForMonsterSkill, GetAssetID, ConvertMonsters)
5. IGameDataManager 인터페이스 수정 (GetMonsterSkillData, GetAllMonsterSkillData)
6. GameDataManager 수정 (LoadMonsterSkillData, _monsterSkillData)
7. Monster.cs 수정 (SetSkills, GetRandomSkill)
8. IBattleManager 이벤트 추가 (OnMonsterHealed, OnMonsterBuffApplied)
9. BattleManager 수정 (ExecuteMonsterSkill 및 서브 메서드, 이벤트)
10. BattleController 수정 (StartTestBattle 스킬 주입, HandleMonsterTurnCoroutine 분기)
11. MonsterSkill.csv 생성
12. MonsterData.csv 수정
13. 테스트 데이터 생성 및 검증

## UI 고려사항

- 전투 로그에 스킬 이름 표시
- 몬스터 스킬 사용 시 이펙트 표시 (기존 AttackEffectUI 활용)
- 몬스터 HP 변화 UI 업데이트 (OnMonsterHealed 이벤트 활용)
- 몬스터 버프 아이콘 표시 (추후 구현)