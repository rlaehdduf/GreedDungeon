---
## Goal

Unity 2D 던전 크롤러 턴제 RPG 프로토타입 개발 (GreedDungeon)

**현재 작업:** 행동 게이지 시스템 & 밸런스 대폭 조정 완료

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
11. **행동 게이지 시스템** - Speed만큼 게이지 증가, 1000 도달 시 행동
12. **레벨업 캐시 무효화** - `InitializeStats()`에서 `_statsCacheDirty = true` 필수

## Accomplished

### 기존 완료
- Phase 1~5 (전투, 속성/상태이상, 인벤토리/버프, 스킬) ✅
- 전투 비주얼 시스템 (턴 딜레이, 공격 모션, 데미지 텍스트, 이펙트, 디버프 비네트) ✅
- 몬스터 특수 스킬 시스템 ✅
- 던전 시스템 (DungeonState, EncounterType, DungeonProgress, EncounterSystem) ✅
- UI 개선 (보물방 툴팁, PlayerStatusUI Level 텍스트) ✅
- PlayerData CSV 데이터화 ✅

### 이번 세션 완료
- ✅ 행동 게이지 시스템 구현 (TurnManager, IBattleEntity, BattleEntity)
- ✅ 레벨업 체력 비율 유지 (Player.cs)
- ✅ 레벨업 스탯 캐시 버그 수정 (BattleEntity.cs)
- ✅ Player 기본 스탯 하향 (HP 70, MP 40, ATK 8, DEF 3)
- ✅ 시작 장비 지급 (막대기 자동 장착)
- ✅ 장비 HP/DEF 50% 감소
- ✅ 몬스터 HP/ATK/DEF 20% 상향
- ✅ Rarity 배율 하향 (Legend 3.0x → 2.0x)
- ✅ 전투 시뮬레이터 작성 (Simulator/BattleSimulator.cs)
- ✅ Game Over UI 구현 (화면 붉어짐, Text 페이드인, 타이틀 이동)

## 시뮬레이션 결과

| 시나리오 | 슬라임 | 역병쥐 | 거미 | 해골 | 켈베로스 |
|----------|--------|--------|------|------|----------|
| 시작 장비 (막대기) | 100% | 27% | 99% | 0% | 0% |
| 일반 장비 | 100% | 100% | 100% | 100% | 0% |
| 전설 장비 | 100% | 100% | 100% | 100% | 31% |
| 전설+스킬 | 100% | 100% | 100% | 100% | 100% |

## CSV 데이터 파일

| 파일 | 설명 |
|------|------|
| PlayerData.csv | 플레이어 기본 스탯 (HP 70, MP 40, ATK 8, DEF 3) |
| MonsterData.csv | 몬스터 데이터 (HP/ATK/DEF 20% 상향) |
| EquipmentData.csv | 장비 (HP/DEF 50% 감소) |
| RarityData.csv | 희귀도 배율 (Legend 2.0x) |
| SkillData.csv | 플레이어 스킬 |
| ConsumableData.csv | 소모품 |
| StatusEffect.csv | 상태이상 |

---