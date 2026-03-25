# 전투 시스템 개선 이력

## 완료된 작업

### 2026-03-25: 버그 수정 및 이벤트 시스템

| 항목 | 상태 | 비고 |
|------|------|------|
| 턴 전환 버그 | ✅ 완료 | ExecuteMonsterTurn → EndTurn |
| MP 부족시 턴 소모 | ✅ 완료 | ExecuteSkill bool 반환 |
| 스킬 쿨타임 +1 | ✅ 완료 | 1턴 쿨타임 = 1턴 대기 |
| 상태이상 이벤트 | ✅ 완료 | OnStatusEffectDamage/Applied/Ended |
| 버프 이벤트 | ✅ 완료 | OnBuffApplied/Ended |
| 전투 로그 연결 | ✅ 완료 | 상태이상/버프 로그 출력 |
| UI 실시간 갱신 | ✅ 완료 | PlayerStatusUI 이벤트 구독 |

---

## 현재 구조 (개선됨)

### 이벤트 시스템
```csharp
// IBattleEntity 이벤트
event Action<IBattleEntity, ActiveStatusEffect, int> OnStatusEffectDamage;
event Action<IBattleEntity, ActiveStatusEffect> OnStatusEffectApplied;
event Action<IBattleEntity, ActiveStatusEffect> OnStatusEffectEnded;
event Action<IBattleEntity, ActiveBuff> OnBuffApplied;
event Action<IBattleEntity, ActiveBuff> OnBuffEnded;

// 흐름
턴 시작 → ProcessTurnStart()
    → 상태이상 데미지 → OnStatusEffectDamage → BattleManager → 로그
    → 상태이상 지속시간 감소 → OnStatusEffectEnded → BattleManager → 로그
턴 종료 → ProcessTurnEnd()
    → 버프 지속시간 감소 → OnBuffEnded → BattleManager → 로그
```

### UI 연결
```
PlayerStatusUI
├── OnDamaged → HP 바 갱신
├── OnStatusEffectApplied/Ended → 디버프 슬롯 갱신
└── OnBuffApplied/Ended → 버프 슬롯 갱신
```

---

## 남은 작업

### 향후 개선사항
- [ ] 전투 상태 머신 (BattleState enum)
- [ ] 행동 시스템 (IAction 인터페이스)
- [ ] 몬스터 AI 개선 (다양한 패턴)
- [ ] 전투 로그 ScrollRect (Unity 설정)