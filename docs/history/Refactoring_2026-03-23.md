# GreedDungeon 리팩토링 보고서
**날짜:** 2026-03-23

## 주요 변경사항

### 1. BattleManager 리팩토링
- **FindStatusEffect 구현:** IGameDataManager 주입 후 상태이상 데이터 조회 기능 추가
- **이벤트 구독 해제:** CleanupPreviousBattle() 메서드로 메모리 누수 방지
- **IBattleManager 인터페이스 개선:** OnBattleStarted, OnMonsterDamaged 이벤트 추가

### 2. DamageCalculator 리팩토링
- **속성 판별 개선:** 문자열 기반(이름 포함 여부) → SkillDataSO.Element 필드 사용
- **SkillDataSO 수정:** Element 필드 추가

### 3. BattleEntity 성능 최적화
- **TotalStats 캐싱:** 매번 Clone() 호출 대신 캐싱 후 버프 변경 시에만 갱신
- **InvalidateStatsCache()**: 버프 추가/제거 시 캐시 무효화

### 4. GameDataManager 중복 코드 제거
- **제네릭 메서드 도입:** LoadDataAsync<T>()로 6개의 LoadXxxAsync() 메서드 통합
- **IData 인터페이스:** 모든 ScriptableObject가 IData 구현

### 5. ScriptableObject 개선
- 모든 데이터 SO에 IData 인터페이스 구현 추가
- SkillDataSO에 Element 필드 추가

## 검토 발견사항 (미수정)

### 높은 우선순위
- AddressablesLoader: 핸들 관리 오류 (Release 시 중복 해제 위험)
- SceneLoader: 동기 씬 로딩만 지원 (비동기 전환 필요)

### 중간 우선순위
- UI 폴더: 컴포넌트 캐싱 부족 (GetComponentsInChildren 반복 호출)
- CSVConverter: 500+ 라인 중복 코드 (제네릭 메서드로 통합 가능)
- UIManager: 싱글톤 vs DI 충돌 가능

### 낮은 우선순위
- 하드코딩된 문자열 (Localization 시스템 도입 권장)
- 매직 넘버 상수화

## 수정된 파일
- Assets/Scripts/Combat/BattleManager.cs
- Assets/Scripts/Combat/BattleController.cs
- Assets/Scripts/Combat/DamageCalculator.cs
- Assets/Scripts/Character/BattleEntity.cs
- Assets/Scripts/Core/GameDataManager.cs
- Assets/Scripts/ScriptableObjects/*.cs (6개 파일)