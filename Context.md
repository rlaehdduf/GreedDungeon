---
## Goal

Unity 2D 던전 크롤러 턴제 RPG 프로토타입 개발 (GreedDungeon)

**현재 작업:** Console Log 정리 및 프로토타입 완성

## Instructions

- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**
- **Pathfinder Core DI 시스템 사용** - MonoBehaviour는 `Services.Get<T>()` 사용
- **SOLID 준수** - 클래스 300행/메서드 10개 제한
- **CSV 데이터 워크플로우**: GameData.xlsm → 자동 CSV 저장 → CSVConverter → ScriptableObject
- **항상 AGENT.md 파일 확인하기**
- **런타임 스크립트에서 Debug.Log 제거** (Editor 스크립트는 유지)

## Discoveries

1. **스킬 MP 차단** - SkillManager에서만 담당 (BattleManager 중복 차감 방지)
2. **스킬 쿨타임** - `Cooldown + 1`로 설정 (1턴 쿨타임 = 1턴 대기)
3. **턴 전환** - ExecuteMonsterTurn 후 EndTurn 호출 필수
4. **이벤트 순서** - 리스트에서 제거 후 이벤트 발행해야 함
5. **Monster 디버프** - 1개만 가능, 새 디버프 적용시 기존 디버프 덮어쓰기
6. **프리팹 구조 분리** - Container(레이아웃만)와 Slot(이미지/텍스트) 분리 필수
7. **장비 스탯 캐시 무효화** - `EquipItem()`, `Unequip()`에서 `InvalidateStatsCache()` 호출 필수
8. **몬스터 스킬 이펙트** - 플레이어 공격에만 `OnAttackEffect` 사용, 몬스터 공격에는 사용하지 않음
9. **BackgroundScroller** - SpriteRenderer용으로 수정, `_bobCycles`로 정확한 sin 사이클 완료
10. **DamageTextUI** - Player 데미지는 빨간색(`_playerDamageColor`)으로 구분

## Accomplished

### 기존 완료
- Phase 1~5 (전투, 속성/상태이상, 인벤토리/버프, 스킬) ✅
- 전투 비주얼 시스템 (턴 딜레이, 공격 모션, 데미지 텍스트, 이펙트, 디버프 비네트) ✅
- 몬스터 특수 스킬 시스템 ✅
- 던전 시스템 (DungeonState, EncounterType, DungeonProgress, EncounterSystem) ✅
- UI 개선 (보물방 툴팁, PlayerStatusUI Level 텍스트) ✅
- PlayerData CSV 데이터화 ✅

### 이번 세션 완료
- ✅ Monster.GetRandomSkill() 유닛 테스트 작성 및 통과
- ✅ TurnManager 유닛 테스트 작성 및 통과
- ✅ TitleUI.cs 작성 (클릭 시 걷는 애니메이션, 텍스트 깜빡임)
- ✅ BackgroundScroller.cs 수정 (SpriteRenderer용, 부드러운 sin 사이클)
- ✅ MonsterDisplay/MonsterStatusUI Show/Hide 기능 추가
- ✅ DamageTextUI Player 데미지 색상 구분 (빨간색)
- ✅ InventorySlotUI 스프라이트 깜빡임 수정
- ✅ DungeonController 테스트 모드 추가 (O키 = 다음 방 상점 고정)

### 진행 중
- 🔄 Debug.Log 제거 (런타임 스크립트만)

## Test Code

### 테스트 키
| 키 | 기능 |
|----|------|
| P | 몬스터 스폰 (BattleController) |
| O | 다음 방 상점 고정 (DungeonController, _testMode 필요) |
| Space | BackgroundScroller 테스트 (BackgroundScroller, _testMode 필요) |

### 유닛 테스트
- `Assets/Tests/Editor/MonsterSkillTests.cs` - 몬스터 스킬 선택 로직
- `Assets/Tests/Editor/TurnManagerTests.cs` - 턴 관리 로직

### 인벤토리 테스트 아이템
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

## CSV 데이터 파일

| 파일 | 설명 |
|------|------|
| PlayerData.csv | 플레이어 기본 스탯 |
| MonsterData.csv | 몬스터 데이터 |
| MonsterSkill.csv | 몬스터 스킬 |
| SkillData.csv | 플레이어 스킬 |
| EquipmentData.csv | 장비 |
| ConsumableData.csv | 소모품 |
| RarityData.csv | 희귀도 |
| StatusEffect.csv | 상태이상 |

---