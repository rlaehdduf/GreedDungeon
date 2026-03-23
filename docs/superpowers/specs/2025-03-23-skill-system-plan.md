# 스킬 시스템 구현 계획

## Phase 1: 인터페이스 및 기본 구조

### Task 1.1: ISkillManager 인터페이스 생성
- 파일: `Assets/Scripts/Skill/ISkillManager.cs`
- 내용: 스킬 풀, 쿨다운, 실행, 패시브 메서드 정의

### Task 1.2: SkillManager 구현
- 파일: `Assets/Scripts/Skill/SkillManager.cs`
- 내용:
  - 스킬 풀 초기화 (IGameDataManager에서 로드)
  - 쿨다운 Dictionary 관리
  - GetRandomSkill() 구현
  - 쿨다운 관련 메서드 구현

## Phase 2: DI 등록 및 스킬 실행

### Task 2.1: GameInstaller 수정
- 파일: `Assets/Scripts/Core/GameInstaller.cs`
- 내용: ISkillManager → SkillManager 등록

### Task 2.2: SkillManager 스킬 실행 구현
- 파일: `Assets/Scripts/Skill/SkillManager.cs`
- 내용:
  - ExecuteSkill() 구현
  - Damage/Buff/Heal 타입별 처리
  - ApplyPassiveStats() 구현

## Phase 3: Player 통합

### Task 3.1: Player 수정
- 파일: `Assets/Scripts/Character/Player.cs`
- 내용:
  - ISkillManager 의존성 추가
  - Equip()에서 GetRandomSkill() 호출
  - GetEquipmentStats()에 패시브 적용

## Phase 4: UI 및 전투 통합

### Task 4.1: BattleController 수정
- 파일: `Assets/Scripts/Combat/BattleController.cs`
- 내용:
  - UseSkill() 메서드 추가
  - 스킬 사용 → 쿨다운 시작

### Task 4.2: SkillSlotUI 수정
- 파일: `Assets/Scripts/UI/Battle/SkillSlotUI.cs`
- 내용:
  - 쿨다운 표시 (텍스트 또는 오버레이)
  - 쿨다운 중 비활성화

### Task 4.3: BattleManager 수정
- 파일: `Assets/Scripts/Combat/BattleManager.cs`
- 내용:
  - EndTurn()에서 ReduceAllCooldowns() 호출

## Phase 5: 테스트

### Task 5.1: Unity에서 테스트
- 장비 장착 → 스킬 획득 확인
- 스킬 사용 → 쿨다운 확인
- 패시브 스탯 반영 확인