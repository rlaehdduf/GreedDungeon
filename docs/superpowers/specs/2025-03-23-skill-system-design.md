# 스킬 시스템 설계

## 개요

턴제 RPG 던전 크롤러에서 스킬 습득, 관리, 사용을 담당하는 시스템.

## 요구사항

### 기능 요구사항
1. 장비 장착 시 SkillPoolType별 랜덤 스킬 획득
2. 전역 쿨다운 (전투 간 유지)
3. 패시브 스킬은 슬롯 장착 시에만 적용
4. 버프 스킬은 사용 시 적용, 지속시간 기반

### 비기능 요구사항
- DI 시스템 (Pathfinder Core) 활용
- SOLID 원칙 준수
- 클래스 300행 제한

## 아키텍처

```
┌─────────────────────────────────────────────────────────┐
│                    SkillManager                          │
│  - ISkillManager 인터페이스 구현                          │
│  - 스킬 풀 관리 (SkillPoolType → List<SkillDataSO>)      │
│  - 전역 쿨다운 추적 (Dictionary<int, int>)               │
│  - 스킬 효과 실행                                         │
└─────────────────────────────────────────────────────────┘
         │                │                │
         ▼                ▼                ▼
   ┌──────────┐    ┌───────────┐    ┌──────────────┐
   │ Player   │    │BattleManager│   │ SkillSlotUI  │
   │ - Skills │    │ - ExecuteAttack│ │ - 슬롯 표시  │
   │ - 쿨다운 │    │              │   │ - 쿨다운 표시│
   └──────────┘    └───────────┘    └──────────────┘
```

## 데이터 구조

### ISkillManager 인터페이스

```csharp
public interface ISkillManager
{
    // 스킬 풀
    SkillDataSO GetRandomSkill(SkillPoolType poolType);
    
    // 쿨다운
    bool IsOnCooldown(int skillId);
    int GetRemainingCooldown(int skillId);
    void StartCooldown(int skillId, int turns);
    void ReduceAllCooldowns();
    void ResetCooldowns();
    
    // 스킬 실행
    void ExecuteSkill(SkillDataSO skill, IBattleEntity caster, IBattleEntity target);
    
    // 패시브 적용
    void ApplyPassiveStats(SkillDataSO skill, Stats stats);
}
```

### SkillManager 구현

**의존성:**
- IGameDataManager (스킬 데이터 로드)

**필드:**
- `Dictionary<SkillPoolType, List<SkillDataSO>> _skillPools`
- `Dictionary<int, int> _cooldowns` (skillId → remaining turns)

### Player 수정

**추가 필드:**
- 없음 (쿨다운은 SkillManager에서 관리)

**수정 메서드:**
- `GetEquipmentStats()`: 패시브 스킬 스탯 적용
- `Equip()`: SkillManager에서 랜덤 스킬 획득

## 스킬 실행 흐름

### 전투에서 스킬 사용

```
1. SkillSlotUI 클릭
       ↓
2. BattleUI.OnSkillSelected → BattleController
       ↓
3. BattleController.UseSkill(skillId)
       ↓
4. ISkillManager.IsOnCooldown() 확인
       ↓ (사용 가능)
5. ISkillManager.ExecuteSkill()
   - Damage: BattleManager.ExecuteAttack() 호출
   - Buff: caster.ApplyBuff() 호출
   - Heal: caster.Heal() 호출
       ↓
6. ISkillManager.StartCooldown()
       ↓
7. UI 갱신 (쿨다운 표시)
```

### 스킬 타입별 처리

| 타입 | 처리 방식 |
|------|----------|
| Common/Melee/Magic | BattleManager.ExecuteAttack() 사용 |
| Buff | ApplyBuff() 호출 (공격력/방어력/속도 증가) |
| Passive | 슬롯 장착 시 TotalStats에 반영 |
| Heal | Heal() 호출, MP 소모 |

### 쿨다운 관리

- 전역 쿨다운: 전투 간 유지
- 턴 종료 시: `BattleManager.EndTurn()` → `ProcessTurnEnd()` 이후 `ReduceAllCooldowns()` 호출
- 던전 진입 시: `ResetCooldowns()` 호출 (Dungeon 씬 진입 시점)

## 파일 구조

```
Assets/Scripts/
├── Skill/
│   ├── ISkillManager.cs         ← 인터페이스 정의
│   └── SkillManager.cs          ← 구현체
│
├── Combat/
│   └── BattleController.cs      ← 스킬 사용 로직 추가
│
├── Character/
│   └── Player.cs                ← 패시브 적용 수정
│
├── UI/Battle/
│   └── SkillSlotUI.cs           ← 쿨다운 표시 추가
│
└── Core/
    └── GameInstaller.cs         ← ISkillManager 등록
```

## 구현 순서

1. ISkillManager 인터페이스 생성
2. SkillManager 구현 (풀, 쿨다운, 실행)
3. GameInstaller에 DI 등록
4. Player 수정 (패시브 적용, 장착 시 스킬 획득)
5. BattleController에 스킬 사용 로직 추가
6. SkillSlotUI에 쿨다운 표시 추가

## 테스트 시나리오

1. 장비 장착 시 스킬 획득 확인
2. 스킬 사용 후 쿨다운 적용 확인
3. 쿨다운 상태에서 사용 불가 확인
4. 패시브 스킬 스탯 반영 확인
5. 버프 스킬 지속시간 확인