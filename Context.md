---
## Goal

던전 크롤러 턴제 RPG 프로토타입 개발 (Unity 2D)

**현재 작업:** 전투 시스템 이벤트 시스템 완료

## Instructions

- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**
- **Pathfinder Core DI 시스템 사용** - MonoBehaviour는 `Services.Get<T>()` 사용
- **SOLID 준수** - 클래스 300행/메서드 10개 제한
- **CSV 데이터 워크플로우**: GameData.xlsm → 자동 CSV 저장 → CSVConverter → ScriptableObject

## Discoveries

1. **순환 의존성** - BattleManager ↔ SkillManager → `Services.Get<T>()`로 해결
2. **비동기 초기화 순서** - `EnsureInitialized()` 지연 초기화 패턴
3. **Unity 코루틴과 Task** - `yield return Task`는 대기하지 않음 → `IsInitialized` 폴링
4. **툴팁 Raycast 차단** - CanvasGroup `blocksRaycasts = false`
5. **Addressables 타입** - Sprite 폴더에 아이콘 배치 후 Addressables 등록
6. **Input System** - 새 Input System 사용 (`UnityEngine.InputSystem`)
7. **스킬 MP 차감** - SkillManager에서만 담당 (BattleManager 중복 차감 방지)
8. **스킬 쿨타임** - `Cooldown + 1`로 설정 (1턴 쿨타임 = 1턴 대기)

9. **턴 전환** - ExecuteMonsterTurn 후 EndTurn 호출 필수

## Accomplished

- Phase 1~4 (전투, 속성/상태이상, 인벤토리/버프) ✅
- Phase 5 스킬 시스템 + 스킬 슬롯 UI + 툴팁 ✅
- 인벤토리 UI 시스템 (InventoryItem, InventorySlotUI, EquipSlotUI, ItemTooltipUI, ConfirmDropPopup) ✅
- 버프/디버프 아이콘 시스템 (StatusEffectSlotUI, Addressables 로드) ✅
- 데이터 워크플로우 (CSV → ScriptableObject, VBA 매크로) ✅
- 인벤토리 UI 수정 (툴팁 Raycast, ESC 닫기, 외부클릭 닫기) ✅
- Input System 새 버전 적용 ✅
- Addressables 자동화 메뉴 (`Tools → Addressables → 🔄 Setup & Build`) ✅
- 인벤토리 소모품 사용 이벤트 연결 ✅
- 테스트 코드 정리 (AddTestItemsToInventory만 유지) ✅
- 인벤토리 닫기 시 ActionMenuUI 재활성화 ✅
- 던전 시스템 설계 문서 작성 ✅
- 장비 시스템 개선 (레벨 시스템, 스탯 표시) ✅
- 전투 종료 감지 및 레벨업 연결 ✅
- 소모품 사용 UI 갱신 수정 ✅
- 장비 스킬 풀 시스템 (장비 타입별 스킬 풀 매칭) ✅
- 툴팁 위치 개선 (화면 경계 내 고정, 자식 요소 bounds 계산) ✅
- 전투 시스템 완성 (몬스터 턴, 전투 로그, 사망 이벤트) ✅
- Rarity Color 적용 (툴팁 이름 색상) ✅
- 전투 로그 간략화 ✅
- 스킬 MP 중복 차감 버그 수정 ✅
- 스킬 쿨타임 매커니즘 수정 (+1) ✅
- 턴 전환 버그 수정 (ExecuteMonsterTurn → EndTurn) ✅
- MP 부족시 턴 미소모 수정 ✅
- 상태이상/버프 이벤트 시스템 구현 ✅
- 전투 로그 실시간 연결 ✅
- PlayerStatusUI 실시간 갱신 ✅

## In Progress

- 없음

## Pending

- 던전 시스템

## Test Code

`BattleController.AddTestItemsToInventory()`:
- 모든 장비 인벤토리 추가
- 모든 소모품 x5 인벤토리 추가
- 골드 1000G 추가

## Icon Addresses

| 타입 | 주소 |
|-----|------|
| 버프 | Skills/PowerBuff, Skills/DefenseBuff, Skills/SpeedBuff |
| 디버프 | Skills/BurnDeBuff, Skills/PoisonDeBuff, Skills/Stun |
| 속성 | Elements/Fire, Elements/Water, Elements/Leaf, Elements/Neutral |

---