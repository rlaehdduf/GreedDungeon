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
- ID: int                       # 스킬 식별자 (기존 패턴 준수)
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
추가 필드:
- UniqueSkillID: int       # 전용 스킬 ID (0 = 없음)
- SharedSkillID: int       # 공용 스킬 ID (0 = 없음)  
- SkillChance: float       # 스킬 발동 확률 (0~100)
```

### CSV 수정

**Monster.csv 추가 컬럼:**
```
UniqueSkillID, SharedSkillID, SkillChance
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

### GameDataManager (수정)

```csharp
// 추가 필드
private Dictionary<int, MonsterSkillDataSO> _monsterSkillData;

// 추가 메서드
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

// InitializeAsync()에서 로드
private async Task LoadMonsterSkillData()
{
    var skills = await CSVConverter.ConvertMonsterSkillData();
    _monsterSkillData = skills?.ToDictionary(s => s.ID);
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
    event Action<int, int> OnMonsterHealed;      // 몬스터 회복 (amount, newHP)
    event Action<BuffType, float, int> OnMonsterBuffApplied;  // 몬스터 버프
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
public static async Task<List<MonsterSkillDataSO>> ConvertMonsterSkillData()
{
    var csvPath = Path.Combine(Application.dataPath, "EditorData/Data/csv/MonsterSkill.csv");
    var lines = await File.ReadAllLinesAsync(csvPath);
    
    var skills = new List<MonsterSkillDataSO>();
    
    for (int i = 1; i < lines.Length; i++)
    {
        var values = ParseCSVLine(lines[i]);
        if (values.Length < 13) continue;
        
        var skill = ScriptableObject.CreateInstance<MonsterSkillDataSO>();
        skill.ID = int.Parse(values[0]);
        skill.Name = values[1];
        skill.Description = values[2];
        skill.SkillType = Enum.Parse<MonsterSkillType>(values[3]);
        skill.IsShared = bool.Parse(values[4]);
        skill.DamageMultiplier = float.Parse(values[5]);
        skill.HitCount = int.Parse(values[6]);
        skill.StatusEffectID = values[7];
        skill.StatusEffectChance = float.Parse(values[8]);
        skill.BuffType = Enum.Parse<BuffType>(values[9]);
        skill.BuffValue = float.Parse(values[10]);
        skill.BuffDuration = int.Parse(values[11]);
        skill.HealPercent = float.Parse(values[12]);
        
        skills.Add(skill);
        
        AssetDatabase.CreateAsset(skill, $"Assets/EditorData/Data/ScriptableObjects/MonsterSkill/MonsterSkill_{skill.ID}.asset");
    }
    
    AssetDatabase.SaveAssets();
    return skills;
}
```

## 실행 흐름

```
GameDataManager.InitializeAsync()
    ↓ LoadMonsterSkillData()
MonsterSkillDataSO 로드 완료
    ↓
BattleController.StartTestBattle()
    ↓
new Monster(data)
    ↓
monster.SetSkills(gameDataManager.GetMonsterSkillData(uniqueID),
                  gameDataManager.GetMonsterSkillData(sharedID))
    ↓
HandleMonsterTurnCoroutine()
    ↓
monster.GetRandomSkill()
    ↓ SkillChance 체크
skill != null → BattleManager.ExecuteMonsterSkill(skill)
skill == null → BattleManager.ExecuteMonsterAttack()
```

## 공용 스킬 예시

| ID | 이름 | 타입 | 효과 |
|----|------|------|------|
| 1 | 강타 | Attack | 150% 데미지 |
| 2 | 연속공격 | Attack | 2회 타격 |
| 3 | 독가스 | Debuff | 중독 30% 확률 |
| 4 | 분노 | Buff | 공격력 +30% 3턴 |
| 5 | 재생 | Heal | HP 20% 회복 |

## 전용 스킬 예시

| ID | 몬스터 | 스킬 | 효과 |
|----|--------|------|------|
| 101 | 슬라임 | 분열 | HP 30% 회복 |
| 102 | 고블린 | 약점공격 | 200% 데미지 |
| 103 | 오크 | 전투함성 | 공격력 +50% 2턴 |
| 104 | 드래곤 | 화염숨 | 2회 타격 + 화상 |

## 구현 순서

1. MonsterSkillDataSO.cs 생성
2. MonsterSkillType enum 정의
3. MonsterDataSO 수정 (UniqueSkillID, SharedSkillID, SkillChance 추가)
4. CSVConverter.ConvertMonsterSkillData() 추가
5. GameDataManager 수정 (LoadMonsterSkillData, GetMonsterSkillData)
6. Monster.cs 수정 (SetSkills, GetRandomSkill)
7. IBattleManager 이벤트 추가 (OnMonsterHealed, OnMonsterBuffApplied)
8. BattleManager 수정 (ExecuteMonsterSkill 및 서브 메서드)
9. BattleController 수정 (스킬 사용 분기)
10. MonsterSkill.csv 생성
11. Monster.csv 수정
12. 테스트 데이터 생성 및 검증

## UI 고려사항

- 전투 로그에 스킬 이름 표시
- 몬스터 스킬 사용 시 이펙트 표시 (기존 AttackEffectUI 활용)
- 몬스터 HP 변화 UI 업데이트 (OnMonsterHealed 이벤트 활용)
- 몬스터 버프 아이콘 표시 (추후 구현)