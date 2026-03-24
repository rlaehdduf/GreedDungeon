# 전투 시스템 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 몬스터 턴, 전투 로그, 사망 이벤트를 구현하여 완전한 턴제 전투 시스템 완성

**Architecture:** 기존 BattleManager/BattleController 구조 활용. 전투 로그는 이벤트 기반으로 BattleLogUI에 전달. 몬스터 턴은 EndTurn()에서 자동 처리.

**Tech Stack:** Unity 2D, C#, 기존 DI 시스템 (Services.Get<T>)

---

## 파일 구조

### 수정 파일
- `Assets/Scripts/Combat/BattleManager.cs` - 몬스터 턴 처리, 전투 로그 이벤트 추가
- `Assets/Scripts/Combat/BattleController.cs` - 로그 UI 연결, 사망 처리
- `Assets/Scripts/UI/Battle/BattleLogUI.cs` - 로그 포맷팅 메서드 추가

### 참조 파일
- `Assets/Scripts/Character/Player.cs` - 사망 확인
- `Assets/Scripts/Character/Monster.cs` - 공격 데이터

---

### Task 1: 전투 로그 이벤트 시스템

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`

- [ ] **Step 1: IBattleManager 인터페이스에 로그 이벤트 추가**

```csharp
// IBattleManager에 추가
event Action<string> OnBattleLog;
```

- [ ] **Step 2: BattleManager에 이벤트 구현**

```csharp
// BattleManager 클래스에 추가
public event Action<string> OnBattleLog;

private void LogBattle(string message)
{
    Debug.Log(message);
    OnBattleLog?.Invoke(message);
}
```

- [ ] **Step 3: 기존 Debug.Log를 LogBattle로 교체**

공격, 데미지, 상태이상 관련 로그를 LogBattle로 변경.

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Combat/BattleManager.cs
git commit -m "feat: 전투 로그 이벤트 시스템 추가"
```

---

### Task 2: 몬스터 턴 구현

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`

- [ ] **Step 1: ExecuteMonsterTurn 메서드 추가**

```csharp
public void ExecuteMonsterTurn()
{
    if (_monster == null || _monster.IsDead || _player == null || _player.IsDead) return;

    LogBattle($"[{_monster.Name}의 턴]");
    
    // 몬스터 기본 공격
    ExecuteMonsterAttack();
    
    // 플레이어 사망 체크
    if (_player.IsDead)
    {
        CheckBattleEnd();
    }
}

private void ExecuteMonsterAttack()
{
    int damage = _monster.BaseStats.Attack;
    
    // 방어 체크
    if (_player.IsDefending)
    {
        damage = damage / 2;
        LogBattle($"  플레이어 방어! 데미지 반감");
    }
    
    // 방어력 적용
    int defense = _player.TotalStats.Defense;
    damage = Mathf.Max(1, damage - defense / 2);
    
    _player.TakeDamage(damage);
    LogBattle($"  {_monster.Name} 공격 → 플레이어 {damage} 데미지");
    
    // 상태이상 적용 (있는 경우)
    TryApplyMonsterStatusEffect();
}

private void TryApplyMonsterStatusEffect()
{
    if (!_monster.HasStatusEffectAttack) return;
    
    var effect = _monster.GetStatusEffectForAttack();
    if (effect == null) return;
    
    float roll = UnityEngine.Random.value * 100;
    if (roll < _monster.GetStatusEffectChance())
    {
        _player.ApplyStatusEffect(effect, effect.Duration);
        LogBattle($"  플레이어 [{effect.Name}] 상태이상 적용!");
    }
}
```

- [ ] **Step 2: EndTurn에서 몬스터 턴 호출**

```csharp
public void EndTurn()
{
    var current = _turnManager.CurrentEntity;
    current?.ProcessTurnEnd();

    if (Services.IsInitialized)
    {
        var skillManager = Services.Get<ISkillManager>();
        skillManager?.ReduceAllCooldowns();
    }

    _turnManager.NextTurn();

    var nextEntity = _turnManager.CurrentEntity;
    if (nextEntity != null)
    {
        nextEntity.ProcessTurnStart();
        if (nextEntity.IsDead)
        {
            CheckBattleEnd();
            return;
        }
        
        // 몬스터 턴 자동 실행
        if (nextEntity == _monster)
        {
            ExecuteMonsterTurn();
        }
    }
}
```

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Combat/BattleManager.cs
git commit -m "feat: 몬스터 턴 및 기본 공격 구현"
```

---

### Task 3: 전투 로그 UI 연결

**Files:**
- Modify: `Assets/Scripts/Combat/BattleController.cs`
- Modify: `Assets/Scripts/UI/Battle/BattleLogUI.cs`

- [ ] **Step 1: BattleController에서 로그 이벤트 구독**

```csharp
// HandleBattleStarted에 추가
_battleManager.OnBattleLog += HandleBattleLog;

// 이벤트 핸들러 추가
private void HandleBattleLog(string message)
{
    if (_battleUI != null)
    {
        _battleUI.AddBattleLog(message);
    }
}

// OnDestroy에서 해제
_battleManager.OnBattleLog -= HandleBattleLog;
```

- [ ] **Step 2: BattleUI에 AddBattleLog 메서드 추가**

```csharp
// BattleUI.cs에 추가
public void AddBattleLog(string message)
{
    if (_battleLog != null)
        _battleLog.AddLog(message);
}
```

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/Combat/BattleController.cs Assets/Scripts/UI/Battle/BattleUI.cs
git commit -m "feat: 전투 로그 UI 연결"
```

---

### Task 4: 턴 표시 로그 추가

**Files:**
- Modify: `Assets/Scripts/Combat/BattleController.cs`

- [ ] **Step 1: 플레이어 행동 시 턴 로그 추가**

HandleAttackClicked, HandleSkillSelected, HandleDefendClicked, HandleItemUsed에 로그 추가.

```csharp
private void HandleAttackClicked()
{
    if (_battleUI != null)
        _battleUI.AddBattleLog("[플레이어의 턴]");
    
    _battleManager.ExecuteAttack(_testPlayer, _currentMonster, null);
    // ... 기존 코드
}
```

- [ ] **Step 2: 커밋**

```bash
git add Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: 플레이어 턴 로그 추가"
```

---

### Task 5: 사망 이벤트 및 게임오버

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`
- Modify: `Assets/Scripts/UI/Battle/BattleUI.cs`

- [ ] **Step 1: IBattleManager에 사망 이벤트 추가**

```csharp
event Action OnPlayerDeath;
event Action OnMonsterDeath;
```

- [ ] **Step 2: BattleManager에서 이벤트 발생**

```csharp
// CheckBattleEnd 수정
private void CheckBattleEnd()
{
    if (!IsBattleOver) return;

    if (PlayerWon)
    {
        LogBattle("═══════════════════════════════════");
        LogBattle("           전투 승리!              ");
        LogBattle("═══════════════════════════════════");
        
        _player.AddKill();
        int goldReward = _monster.GoldDrop;
        _player.AddGold(goldReward);
        LogBattle($"  획득 골드: {goldReward}G");
        
        OnMonsterDeath?.Invoke();
    }
    else
    {
        LogBattle("═══════════════════════════════════");
        LogBattle("           전투 패배...            ");
        LogBattle("═══════════════════════════════════");
        
        OnPlayerDeath?.Invoke();
    }
}
```

- [ ] **Step 3: BattleUI에 게임오버 표시 메서드 추가**

```csharp
public void ShowGameOver()
{
    AddBattleLog("게임 오버!");
    EnableActions(false);
}
```

- [ ] **Step 4: BattleController에서 사망 이벤트 처리**

```csharp
_battleManager.OnPlayerDeath += HandlePlayerDeath;
_battleManager.OnMonsterDeath += HandleMonsterDeath;

private void HandlePlayerDeath()
{
    _battleUI?.ShowGameOver();
}

private void HandleMonsterDeath()
{
    // 다음 전투 준비 (던전 시스템에서 처리)
}
```

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/Combat/BattleManager.cs Assets/Scripts/Combat/BattleController.cs Assets/Scripts/UI/Battle/BattleUI.cs
git commit -m "feat: 사망 이벤트 및 게임오버 구현"
```

---

### Task 6: 아이템 사용 턴 소모

**Files:**
- Modify: `Assets/Scripts/Combat/BattleController.cs`

- [ ] **Step 1: HandleItemUsed 확인**

이미 EndTurn()이 호출되고 있음. 장비 변경은 인벤토리에서 처리하므로 EndTurn 호출 안 함.

현재 코드 확인:
```csharp
private void HandleItemUsed(Items.InventoryItem item)
{
    // ...
    _battleManager.ExecuteItem(item, target);
    _battleManager.EndTurn();  // 이미 턴 소모됨
    // ...
}
```

- [ ] **Step 2: 인벤토리에서 장비 변경 시 턴 소모 안 함 확인**

InventoryUI의 장비 장착 이벤트는 EndTurn을 호출하지 않아야 함. 현재 구조 확인 필요.

- [ ] **Step 3: 커밋 (변경사항 있는 경우)**

---

## 테스트 방법

1. Unity에서 게임 실행
2. 전투 시작 후 로그 확인:
   - `[플레이어의 턴]` 표시
   - 공격 시 데미지 로그
   - `[몬스터의 턴]` 표시
   - 몬스터 공격 로그
3. 몬스터 처치 시 승리 로그
4. 플레이어 사망 시 패배 로그

---

## 의존성

- 기존 BattleManager, BattleController 구조
- TurnManager (턴 순서 관리)
- Player, Monster 클래스