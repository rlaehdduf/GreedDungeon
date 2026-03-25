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
- ID: string                    # 스킬 식별자
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
- UniqueSkillID: string   # 전용 스킬 ID (1개)
- SharedSkillID: string   # 공용 스킬 ID (1개)  
- SkillChance: float      # 스킬 발동 확률 (0~100)
```

### CSV 수정 (Monster.csv)

```
추가 컬럼:
- UniqueSkillID
- SharedSkillID
- SkillChance
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
    public string ID;
    public string Name;
    [TextArea] public string Description;
    public MonsterSkillType SkillType;
    public bool IsShared;
    
    // Attack
    public float DamageMultiplier = 1f;
    public int HitCount = 1;
    
    // Debuff
    public string StatusEffectID;
    [Range(0f, 100f)] public float StatusEffectChance;
    
    // Buff
    public BuffType BuffType;
    public float BuffValue;
    public int BuffDuration;
    
    // Heal
    public float HealPercent;
    
    int IData.ID => int.TryParse(ID, out int id) ? id : 0;
}
```

### Monster (수정)

```csharp
public class Monster : BattleEntity
{
    private MonsterSkillDataSO _uniqueSkill;
    private MonsterSkillDataSO _sharedSkill;
    
    public MonsterSkillDataSO UniqueSkill => _uniqueSkill;
    public MonsterSkillDataSO SharedSkill => _sharedSkill;
    public float SkillChance => _data.SkillChance;
    
    public void SetSkills(MonsterSkillDataSO unique, MonsterSkillDataSO shared)
    {
        _uniqueSkill = unique;
        _sharedSkill = shared;
    }
    
    public MonsterSkillDataSO GetRandomSkill()
    {
        if (UnityEngine.Random.value * 100 > _data.SkillChance)
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

### BattleManager (수정)

```csharp
public void ExecuteMonsterSkill(MonsterSkillDataSO skill)
{
    switch (skill.SkillType)
    {
        case MonsterSkillType.Attack:
            ExecuteAttackSkill(skill);
            break;
        case MonsterSkillType.Buff:
            ExecuteBuffSkill(skill);
            break;
        case MonsterSkillType.Debuff:
            ExecuteDebuffSkill(skill);
            break;
        case MonsterSkillType.Heal:
            ExecuteHealSkill(skill);
            break;
    }
}

private void ExecuteAttackSkill(MonsterSkillDataSO skill)
{
    int baseDamage = _monster.BaseStats.Attack;
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
```

## 실행 흐름

```
BattleController.HandleMonsterTurnCoroutine()
    ↓
Monster.GetRandomSkill()
    ↓ SkillChance 체크
null → BattleManager.ExecuteMonsterAttack() (기본 공격)
skill → BattleManager.ExecuteMonsterSkill(skill)
```

## 공용 스킬 예시

| ID | 이름 | 타입 | 효과 |
|----|------|------|------|
| MS001 | 강타 | Attack | 150% 데미지 |
| MS002 | 연속공격 | Attack | 2회 타격 |
| MS003 | 독가스 | Debuff | 중독 30% 확률 |
| MS004 | 분노 | Buff | 공격력 +30% 3턴 |
| MS005 | 재생 | Heal | HP 20% 회복 |

## 전용 스킬 예시

| 몬스터 | 스킬 | 효과 |
|--------|------|------|
| 슬라임 | 분열 | HP 30% 회복 |
| 고블린 | 약점공격 | 200% 데미지 |
| 오크 | 분노 | 공격력 +50% |
| 드래곤 | 화염숨 | 2회 타격 + 화상 |

## 구현 순서

1. MonsterSkillDataSO.cs 생성
2. MonsterDataSO 수정 (필드 추가)
3. Monster.cs 수정 (스킬 로직)
4. BattleManager 수정 (ExecuteMonsterSkill)
5. BattleController 수정 (스킬 사용 분기)
6. CSV 데이터 추가
7. CSVConverter 수정
8. 테스트 데이터 생성

## UI 고려사항

- 전투 로그에 스킬 이름 표시
- 몬스터 스킬 사용 시 이펙트 표시 (기존 AttackEffectUI 활용)