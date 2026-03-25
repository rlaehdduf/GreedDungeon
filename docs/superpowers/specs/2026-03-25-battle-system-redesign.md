# 전투 시스템 고퀄리티 재제작 계획

## 현재 문제점

### 1. 턴 시스템
- ExecuteMonsterTurn 후 EndTurn 미호출로 턴 전환 실패
- 턴 관리가 BattleManager와 TurnManager에 분산

### 2. 스킬 시스템
- ExecuteSkill 반환값 없어 실패 여부 확인 불가
- MP 부족시에도 턴 소모
- 쿨타임 매커니즘이 +1로 해결 (근본적 해결 아님)

### 3. 전투 로그
- 로그가 화면 밖으로 나감 (ScrollRect 미설정)
- 로그 메시지가 너무 길어짐

### 4. 상태이상 시스템
- 화상/독/스턴이 턴 시작시 처리되지 않음
- 지속시간 감소가 제대로 동작하지 않음

### 5. 버프/디버프 UI
- 실제 적용된 버프가 UI에 표시되지 않음

---

## 재제작 목표

### 1. 명확한 턴 흐름
```
전투 시작
    ↓
[턴 시작]
    ├── 상태이상 처리 (데미지, 지속시간 감소)
    ├── 행동 선택 (공격/스킬/방어/아이템)
    ├── 행동 실행
    └── [턴 종료]
            ↓
        다음 엔티티 턴
```

### 2. 전투 상태 머신
```
enum BattleState
{
    Init,           // 전투 초기화
    PlayerTurn,     // 플레이어 턴
    PlayerAction,   // 플레이어 행동 중
    MonsterTurn,    // 몬스터 턴
    MonsterAction,  // 몬스터 행동 중
    Victory,        // 승리
    Defeat          // 패배
}
```

### 3. 이벤트 기반 구조
```csharp
// 모든 전투 이벤트를 하나의 시스템에서 관리
public interface IBattleEventBus
{
    event Action<TurnStartEvent> OnTurnStart;
    event Action<TurnEndEvent> OnTurnEnd;
    event Action<DamageEvent> OnDamage;
    event Action<HealEvent> OnHeal;
    event Action<StatusEffectEvent> OnStatusEffect;
    event Action<BuffEvent> OnBuff;
}
```

### 4. 전투 로그 개선
- ScrollRect + Viewport + Content 구조 필수
- 로그 타입별 색상/아이콘
- 최대 줄 수 제한 (자동 삭제)
- 애니메이션 효과 (새 로그 슬라이드 인)

---

## 새로운 구조

### 파일 구조
```
Assets/Scripts/Combat/
├── Core/
│   ├── BattleSystem.cs          // 메인 전투 관리 (상태 머신)
│   ├── BattleState.cs           // 전투 상태 enum
│   └── BattleEventBus.cs        // 이벤트 버스
├── Turn/
│   ├── TurnManager.cs           // 턴 순서 관리
│   └── TurnActions.cs           // 턴 행동 정의
├── Actions/
│   ├── AttackAction.cs          // 공격 행동
│   ├── SkillAction.cs           // 스킬 행동
│   ├── DefendAction.cs          // 방어 행동
│   └── ItemAction.cs            // 아이템 행동
├── Effects/
│   ├── StatusEffectProcessor.cs // 상태이상 처리
│   └── BuffProcessor.cs         // 버프 처리
└── UI/
    ├── BattleUIController.cs    // UI 컨트롤러
    └── BattleLogController.cs   // 로그 컨트롤러
```

---

## 작업 단계

### Phase 1: 기반 구조 (1-2일)
- [ ] BattleState enum 정의
- [ ] BattleEventBus 인터페이스/구현
- [ ] BattleSystem 클래스 (상태 머신)

### Phase 2: 턴 시스템 재구현 (1일)
- [ ] TurnManager 개선
- [ ] 턴 시작/종료 이벤트
- [ ] 상태이상 처리 통합

### Phase 3: 행동 시스템 (1-2일)
- [ ] IAction 인터페이스
- [ ] 각 행동 클래스 구현
- [ ] 행동 실행 → 이벤트 발행

### Phase 4: UI 연결 (1일)
- [ ] BattleUIController
- [ ] 이벤트 구독 → UI 업데이트
- [ ] BattleLogController (ScrollRect 필수)

### Phase 5: 테스트 및 정리 (1일)
- [ ] 기존 BattleController 제거
- [ ] 통합 테스트
- [ ] 문서 업데이트

---

## 우선순위

1. **즉시 수정** (오늘)
   - 턴 전환 버그 수정
   - MP 부족시 턴 미소모

2. **단기 개선** (이번 주)
   - 전투 로그 ScrollRect 설정
   - 상태이상 턴 시작 처리

3. **중기 재설계** (다음 주)
   - 전체 구조 재설계
   - 이벤트 기반 시스템 도입