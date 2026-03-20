# 📝 개발 태스크 리스트

체크리스트 형태로 관리하세요. 완료된 항목은 `[x]`로 표시하세요.

---

## Phase 1: 전투 시스템 기초 (2-3일) ✅ 완료

### 1.1 프로젝트 설정
- [x] Unity 프로젝트 생성 (2D)
- [x] 폴더 구조 생성 (Scripts, Prefabs, Scenes, ScriptableObjects, Art, Tilemaps)
- [x] 씬 생성 (Title, Dungeon, Battle)

### 1.2 코어 클래스
- [x] `GameManager.cs` - 게임 상태 관리 (싱글톤)
- [x] `SceneLoader.cs` - 씬 전환 관리

### 1.3 캐릭터 기초
- [x] `Stats.cs` - 스탯 클래스 (HP, MP, 공격력, 방어력, 마공, 마방, 속도, 크리티컬)
- [x] `BattleEntity.cs` - 전투 엔티티 기본 클래스
  - [x] 현재 HP/MP
- [x] `Player.cs` - 플레이어 클래스 (BattleEntity 상속)
- [x] `Monster.cs` - 몬스터 클래스 (BattleEntity 상속)

### 1.4 턴 시스템
- [x] `TurnManager.cs` - 턴 관리
  - [x] 턴 순서 계산 (속도 기반)
  - [x] 현재 턴 엔티티 관리
  - [x] 턴 시작/종료 이벤트

### 1.5 전투 매니저
- [x] `BattleManager.cs` - 전투 전체 관리
  - [x] 전투 시작
  - [x] 전투 종료 (승리/패배)
  - [x] 플레이어/몬스터 참조

### 1.6 데미지 시스템
- [x] `DamageCalculator.cs` - 데미지 계산
  - [x] 기본 데미지 계산
  - [x] 크리티컬 계산

### 1.7 기본 액션
- [x] 공격 액션 구현
- [x] 방어 액션 구현 (데미지 50% 감소)

### 1.8 테스트
- [x] 플레이어 vs 몬스터 기본 전투 테스트

---

## Phase 2: 속성 & 상태이상 시스템 (1-2일) ✅ 완료

### 2.1 속성 시스템
- [x] `Element.cs` - 속성 enum (불, 물, 풀)
- [x] 속성 상성 계산 (불 > 풀 > 물 > 불)
- [x] 데미지 계산에 속성 상성 적용

### 2.2 상태이상 기초
- [x] `StatusEffectDataSO.cs` - 상태이상 데이터 (ScriptableObject)
  - [x] 지속시간
  - [x] 효과 적용 메서드
- [x] `ActiveStatusEffect.cs` - 상태이상 인스턴스
  - [x] 상태이상 추가/제거
  - [x] 턴 시작 시 효과 적용

### 2.3 개별 상태이상
- [x] **Poison** - 중독 (턴당 HP % 감소)
- [x] **Burn** - 화상 (턴당 HP % 감소)
- [x] **Stun** - 스턴 (행동 불가)

### 2.4 테스트
- [x] 속성 상성 테스트
- [x] 각 상태이상 테스트

---

## Phase 3: UI 시스템 (1-2일)

### 3.1 전투 UI
- [ ] `BattleUI.cs` - 전투 UI 전체 관리
- [ ] `HPBar.cs` - 체력바 (슬라이더)
- [ ] `MPBar.cs` - 마나바 (슬라이더)
- [ ] `ActionButton.cs` - 행동 버튼 (공격, 방어, 스킬, 아이템)
- [ ] `SkillPanel.cs` - 스킬 선택 패널
- [ ] `BattleLog.cs` - 전투 메시지 표시

### 3.2 상태이상 UI
- [ ] `StatusIcon.cs` - 상태이상 아이콘 표시

### 3.3 던전 UI
- [ ] `DungeonUI.cs` - 던전 탐사 UI
  - [ ] 미니맵 (선택)
  - [ ] 플레이어 상태 표시

### 3.4 테스트
- [ ] UI 반응성 테스트

---

## Phase 4: 장비 & 인벤토리 시스템 (2-3일) 🔄 진행중

### 4.1 장비 데이터
- [x] `EquipmentType.cs` - 장비 타입 enum (무기, 갑옷, 악세서리)
- [x] `EquipmentDataSO.cs` - 장비 데이터 (ScriptableObject)
  - [x] 장비 이름
  - [x] 장비 타입
  - [x] 기본 스탯

### 4.2 장비 슬롯
- [x] `Player.cs` 내 장비 슬롯 관리
  - [x] 무기 슬롯
  - [x] 갑옷 슬롯
  - [x] 악세서리 슬롯
- [x] 장비 장착/해제
- [x] 장비 장착 시 스탯 적용
- [ ] 장비 장착 시 스킬 활성화

### 4.3 인벤토리
- [x] `ConsumableItem.cs` - 소모품 인스턴스
- [x] `Player.cs` 인벤토리 추가
  - [x] 아이템 추가/제거
  - [x] 아이템 사용

### 4.4 버프 시스템
- [x] `ActiveBuff.cs` - 버프 상태 클래스
- [x] 버프 타입 (Attack, Defense, Speed)
- [x] 버프 지속시간 관리
- [x] TotalStats에 버프 적용

### 4.5 아이템 사용
- [x] `BattleManager.ExecuteItem()` - 아이템 사용 로직
- [x] Heal - HP 회복
- [x] Cleanse - 디버프 해제
- [x] Buff - 스탯 증가
- [x] Poison/Burn - 적에게 상태이상
- [x] Attack - 적에게 데미지

### 4.6 장비 생성
- [ ] `EquipmentGenerator.cs` - 장비 랜덤 생성
  - [ ] 등급 랜덤 결정
  - [ ] 스킬 랜덤 부여 (무기: 액티브, 갑옷: 패시브, 악세서리: 둘 다)
  - [ ] 스탯 배율 적용

### 4.7 장비 UI
- [ ] `InventoryUI.cs` - 인벤토리 UI
  - [ ] 장비 리스트 표시
  - [ ] 장비 상세 정보
  - [ ] 장비 장착/해제 버튼
- [ ] `EquipmentSlotUI.cs` - 장착 중인 장비 표시

### 4.8 테스트
- [x] 아이템 사용 테스트
- [ ] 장비 생성 테스트
- [ ] 장비 장착 시 스탯 변화 테스트
- [ ] 스킬 활성화 테스트

---

## Phase 5: 스킬 시스템 (2일)

### 5.1 스킬 기초
- [x] `SkillDataSO.cs` - 스킬 데이터 (ScriptableObject)
  - [x] 스킬 이름
  - [x] 스킬 타입 (액티브/패시브)
  - [x] MP 소모
  - [x] 효과 실행

### 5.2 스킬 데이터
- [x] 15종 스킬 데이터 변환 완료

### 5.3 액티브 스킬 구현
- [ ] **파워 슬래시** - 150% 물리 데미지
- [ ] **파이어볼** - 130% 마법 데미지 + 화상
- [ ] **치유** - HP 30 회복
- [ ] **철벽방어** - 1턴간 데미지 50% 감소

### 5.4 패시브 스킬 구현
- [ ] **강철 의지** - 방어력 +20%
- [ ] **전투의 함성** - 공격력 +15%
- [ ] **마나 흐름** - MP +30
- [ ] **재생** - 매 턴 HP 5 회복
- [ ] **마나 회복** - 매 턴 MP 3 회복
- [ ] **긴급 회복** - HP 20% 이하시 1회 HP 30 회복
- [ ] **반격** - 공격받을시 30% 확률 반격
- [ ] **최후의 저항** - HP 30% 이하시 방어력 +50%

### 5.5 테스트
- [ ] 액티브 스킬 사용 테스트
- [ ] 패시브 스킬 적용 테스트

---

## Phase 6: 몬스터 시스템 (1일) ✅ 데이터 완료

### 6.1 몬스터 데이터
- [x] `MonsterDataSO.cs` - 몬스터 데이터 (ScriptableObject)
  - [x] 몬스터 이름
  - [x] 속성
  - [x] 기본 스탯
  - [x] 드롭 테이블

### 6.2 몬스터 구현 (5종)
- [x] **슬라임** - 물속성, 기본 몬스터
- [x] **골렘** - 불속성, 강함
- [x] **좀비** - 풀속성, 중독
- [x] **해골** - 불속성, 빠름
- [x] **킹슬라임** (보스) - 물속성, 강력

### 6.3 몬스터 스프라이트 애니메이션 ✅
- [x] `MonsterSpriteView.cs` - 몬스터 스프라이트 뷰
  - [x] 숨쉬기 애니메이션 (Scale Y축 변화)
  - [x] 데미지 애니메이션 (Scale 작아짐 + 빨간색)
- [x] `MonsterDataSO.cs` - ScaleX, ScaleY 필드 추가
- [x] `BattleEntity.cs` - OnDamaged 이벤트 추가
- [x] `CSVConverter.cs` - Scale 필드 파싱 추가
- [x] `SpriteAddressablesSetter.cs` - 스프라이트 Addressables 설정
- [ ] MonsterData.csv에 ScaleX, ScaleY 데이터 입력 필요

### 6.4 몬스터 AI
- [ ] `MonsterAI.cs` - 몬스터 행동 AI
  - [ ] HP > 50%: 공격 우선
  - [ ] HP 30-50%: 스킬 사용 확률 증가
  - [ ] HP < 30%: 방어 확률 증가

### 6.5 테스트
- [x] 각 몬스터 전투 테스트
- [ ] 보스 전투 테스트
- [x] 스페이스바 데미지 애니메이션 테스트 (AddressablesTest 씬)

---

## Phase 7: 던전 시스템 (2-3일)

### 7.1 던전 매니저
- [ ] `DungeonManager.cs` - 던전 전체 관리
- [ ] `EncounterManager.cs` - 인카운터 관리
  - [ ] 카운트 게이지 시스템
  - [ ] 전투 확률 계산

### 7.2 타일맵 던전
- [ ] 던전 타일맵 생성
- [ ] 플레이어 이동 구현
- [ ] 벽/장애물 충돌

### 7.3 던전 요소
- [ ] 상자 배치 및 상호작용
- [ ] 상점 NPC 배치 및 상호작용
- [ ] 회복 포인트 배치 및 상호작용
- [ ] 보스방 진입

### 7.4 씬 전환
- [ ] 던전 → 전투 씬 전환
- [ ] 전투 → 던전 씬 복귀

### 7.5 테스트
- [ ] 던전 탐사 테스트
- [ ] 인카운터 테스트
- [ ] 상자/상점/회복 포인트 테스트

---

## Phase 8: 상점 & 보상 시스템 (1일)

### 8.1 상점 시스템
- [ ] `Shop.cs` - 상점 관리
- [ ] 상점 UI
- [ ] 랜덤 아이템 생성
- [ ] 구매 기능

### 8.2 보상 시스템
- [ ] 전투 보상 (골드, 장비)
- [ ] 상자 보상 (골드, 장비)
- [ ] 보스 처치 보상
- [ ] 던전 클리어 보상

### 8.3 골드 시스템
- [x] 플레이어 골드 관리
- [ ] 장비 판매 기능

### 8.4 테스트
- [ ] 상점 구매 테스트
- [ ] 장비 판매 테스트
- [ ] 보상 테스트

---

## Phase 9: 통합 & 테스트 (1-2일)

### 9.1 타이틀 씬
- [ ] 타이틀 UI
- [ ] 게임 시작 버튼

### 9.2 게임 루프
- [ ] 게임 오버 처리
- [ ] 던전 클리어 처리
- [ ] 재시작 기능

### 9.3 통합 테스트
- [ ] 전체 게임 플레이 테스트
- [ ] 밸런스 조정
- [ ] 버그 수정

### 9.4 마무리
- [ ] 코드 정리
- [ ] 주석 추가

---

## 진행 상황

| Phase | 상태 | 완료일 |
|-------|------|--------|
| Phase 1 | ✅ 완료 | 2026-03-19 |
| Phase 2 | ✅ 완료 | 2026-03-19 |
| Phase 3 | ⬜ 미시작 | - |
| Phase 4 | 🔄 진행중 | - |
| Phase 5 | ⬜ 미시작 | - |
| Phase 6 | 🔄 데이터 완료 | - |
| Phase 7 | ⬜ 미시작 | - |
| Phase 8 | ⬜ 미시작 | - |
| Phase 9 | ⬜ 미시작 | - |

---

## 메모

- 진행 중 발생하는 이슈나 아이디어를 여기에 기록하세요.

### 발견된 이슈
1. **Unity CLI 연결 문제** - HTTP server는 실행되지만 명령이 타임아웃됨. 파일 조작으로 우회.
2. **CSV 파싱 UTF-8 BOM 문제 해결**: `File.ReadAllBytes()`로 직접 읽고 UTF-8 BOM 감지 후 수동 디코딩
3. **생성자 순서 버그**: 추상 클래스에서 `abstract property`는 자식 생성자보다 늦게 초기화됨 → `InitializeStats()` 패턴으로 해결
4. **Resources 폴더 보안 이슈**: Resources 폴더의 모든 파일은 빌드에 무조건 포함되어 사용자가 추출 가능 → EditorData 폴더로 이동하여 해결

---

## Phase 10: Addressables & 리소스 보안 (추후 진행)

### 10.1 폴더 구조 마이그레이션
- [x] CSV 파일 이동: `Assets/Resources/Data/` → `Assets/EditorData/Data/`
- [x] ScriptableObject 폴더 구조 정리
- [ ] Resources 폴더 정리 (불필요한 파일 제거)

### 10.2 Addressables 설정
- [ ] Addressables 그룹 생성
  - [ ] `MonsterIcons` 그룹
  - [ ] `ItemIcons` 그룹
  - [ ] `StatusEffectIcons` 그룹
  - [ ] `EquipmentIcons` 그룹
- [ ] Addressables 라벨 설정 (Monster, Item, Buff, Debuff 등)

### 10.3 리소스 에셋 준비
- [ ] 몬스터 아이콘 (5종)
  - [ ] 슬라임
  - [ ] 골렘(불)
  - [ ] 좀비
  - [ ] 해골
  - [ ] 킹슬라임 (보스)
- [ ] 장비 아이콘 (15종)
  - [ ] 무기 (5종): 막대기, 검, 방패, 완드, 너클
  - [ ] 갑옷 (5종): 가죽갑옷, 사슬갑옷, 강철갑옷, 가시갑옷, 깃털갑옷
  - [ ] 장신구 (5종): 반지, 목걸이, 망토, 귀걸이, 팔찌
- [ ] 소모품 아이콘 (10종)
  - [ ] 회복포션 소/중/대
  - [ ] 해독제
  - [ ] 힘의물약, 철의물약
  - [ ] 저주의물약, 화염병
  - [ ] 마법화살, 폭탄
- [ ] 상태이상 아이콘 (3종)
  - [ ] 화상 (Burn)
  - [ ] 중독 (Poison)
  - [ ] 기절 (Stun)

### 10.4 데이터 로드 방식 변경
- [ ] `Resources.Load()` → `Addressables.LoadAssetAsync()` 변경
- [ ] `DataManager.cs` 생성 (Addressables 래퍼)
- [ ] 비동기 로딩 처리

### 10.5 빌드 테스트
- [ ] Addressables 빌드
- [ ] 실제 빌드에서 데이터 노출 확인
- [ ] CSV/엑셀 파일 미포함 확인

### 10.6 참고 사항
```
현재 구조:
Assets/
├── EditorData/Data/              ← CSV/엑셀 (빌드 제외)
│   ├── GameData.xlsx
│   ├── MonsterData.csv
│   └── ...
│
├── ScriptableObjects/Data/       ← 변환된 데이터 (빌드 포함)
│   ├── Monsters/        (5개)
│   ├── Skills/          (15개)
│   ├── Equipments/      (15개)
│   ├── Consumables/     (10개)
│   ├── Rarities/        (5개)
│   └── StatusEffects/   (3개)
│
└── AddressableAssetsData/        ← Addressables 패키지 설치됨
```