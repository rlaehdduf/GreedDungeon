---
## Goal

던전 크롤러 턴제 RPG 프로토타입 개발 (Unity 2D)

**현재 작업:** PlayerData CSV 데이터화 완료

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
10. **Container 왼쪽 정렬** - Anchor Min/Max (0, 0.5), Pivot (0, 0.5), ChildAlignment (3)
11. **PlayerData 캐싱** - Player 생성자에서 Services 미초기화 가능 → static 캐싱 사용

## Accomplished

### 기존 완료
- Phase 1~5 (전투, 속성/상태이상, 인벤토리/버프, 스킬) ✅
- 전투 비주얼 시스템 (턴 딜레이, 공격 모션, 데미지 텍스트, 이펙트, 디버프 비네트) ✅
- 몬스터 특수 스킬 시스템 ✅

### 던전 시스템 ✅
- DungeonState, EncounterType, DungeonProgress, EncounterSystem
- BackgroundScroller, DungeonController
- TreasurePopupUI, ShopUI
- BattleController와 DungeonController 연결

### UI 개선
- 보물방 아이템 툴팁 표시 ✅
- PlayerStatusUI Level 텍스트 추가 ✅

### 데이터 시스템
- PlayerDataSO 생성 (플레이어 기본 스탯 CSV 데이터화) ✅
- PlayerData.csv 템플릿 생성 ✅

## Pending

- Unity Editor 작업:
  - `Tools → CSV → Convert All` 실행
  - `Tools → Addressables → 🔄 Setup & Build` 실행

## CSV 데이터 파일

| 파일 | 설명 |
|------|------|
| PlayerData.csv | 플레이어 기본 스탯 (HP, MP, ATK, DEF, SPD, CRIT, 레벨업 보너스 등) |
| MonsterData.csv | 몬스터 데이터 |
| MonsterSkill.csv | 몬스터 스킬 |
| SkillData.csv | 플레이어 스킬 |
| EquipmentData.csv | 장비 |
| ConsumableData.csv | 소모품 |
| RarityData.csv | 희귀도 |
| StatusEffect.csv | 상태이상 |

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
| 이펙트 | Effects/Neutral, Effects/Melee, Effects/Magic |

---