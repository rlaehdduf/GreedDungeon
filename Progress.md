# GreedDungeon 개발 진행상황

## 목표
던전 크롤러 턴제 RPG 프로토타입 개발 (Unity 2D)

## 개발 규칙
- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**
- **Pathfinder Core DI 시스템 사용** - MonoBehaviour는 `Services.Get<T>()` 사용
- **SOLID 준수** - 클래스 300행/메서드 10개 제한
- **순환 의존성 주의** - DI 생성자 주입 대신 `Services.Get<T>()` 런타임 호출로 해결
- **CSV 데이터 워크플로우**: GameData.xlsm → 자동 CSV 저장 → CSVConverter → ScriptableObject

## 발견 사항 (Discoveries)
1. **순환 의존성 문제** - BattleManager와 SkillManager가 서로 필요. 해결: `Services.Get<T>()` 런타임 호출
2. **비동기 초기화 순서** - SkillManager가 GameDataManager보다 먼저 생성되어 스킬 풀이 비어있었음. 해결: `EnsureInitialized()` 지연 초기화 패턴
3. **Unity 코루틴과 Task** - `yield return Task`는 실제로 대기하지 않음. `IsInitialized` 폴링으로 해결
4. **툴팁 Raycast 차단** - CanvasGroup `blocksRaycasts = false`로 설정하여 마우스 이벤트 통과
5. **Addressables 타입 문제** - 스킬 아이콘이 Sprite 타입으로 등록되지 않아 로드 실패. 해결: 별도 Sprite 폴더에 아이콘 배치
6. **Pathfinder DI [Inject] 제한** - `[Inject]` 속성은 DI 컨테이너가 생성한 객체에서만 작동. MonoBehaviour는 `Services.Get<T>()` 사용

## 완료된 작업 (Accomplished)

### Phase 1: 전투 시스템 기초 ✅
- 턴제 전투 시스템 구현
- 기본 스탯 시스템 (HP, MP, 공격력, 방어력, 속도, 크리티컬)
- 전투 로그 시스템

### Phase 2: 속성 & 상태이상 ✅
- 5가지 속성 시스템 (무, 불, 물, 풀, 전기)
- 상태이상 시스템 (화상, 독, 스턴)
- 속성 상성 보너스

### Phase 3: UI 시스템 ✅
- UI Core 시스템 (UIManager, UIView)
- 전투 UI (BattleUI, PlayerStatusUI, MonsterStatusUI, ActionMenuUI, BattleLogUI)
- 스킬 슬롯 UI + 툴팁
- 인벤토리 UI (InventoryUI, InventorySlotUI, EquipSlotUI, ItemTooltipUI, ConfirmDropPopup)
- 타이틀 UI (TitleUI)
- 팝업 시스템 (ConfirmPopup)

### Phase 4: 인벤토리/버프/아이템 사용 ✅
- 인벤토리 시스템 (InventoryItem 20칸)
- 버프/디버프 시스템
- 소모품 아이템 사용

### Phase 5: 스킬 시스템 ✅
- 스킬 데이터 구조 (SkillDataSO)
- 스킬 풀 시스템 (SkillManager)
- 스킬 아이콘 Addressables 등록 자동화

### 버프/디버프 아이콘 시스템 ✅
- StatusEffectSlotUI: 아이콘 + Count 텍스트 컴포넌트
- PlayerStatusUI: _debuffSlots, _buffSlots 리스트 (풀 방식)
- MonsterStatusUI: _debuffSlots 리스트 (풀 방식)
- StatusEffectDataSO: IconAddress 필드 추가
- ActiveBuff: GetIconAddress() 메서드 추가

### 데이터 워크플로우 구축 ✅
- CSV 파일 구조 정리: `Assets/EditorData/Data/csv/`
- CSVConverter: Rarity ColorHex, StatusEffect IconAddress 파싱
- VBA 매크로: 엑셀 저장 시 자동 CSV 생성

### Addressables 런타임 로드 시스템 ✅
- IAssetLoader 인터페이스 및 AddressablesLoader 구현
- IGameDataManager 인터페이스 및 GameDataManager 구현
- Services 정적 클래스 (서비스 로케이터 패턴)
- 속성 아이콘 Addressables 등록 (Elements/Fire, Elements/Water, Elements/Leaf, Elements/Neutral)

### Monster Sprite Animation System ✅
- MonsterDataSO: ScaleX, ScaleY 필드 추가
- MonsterSpriteView: 숨쉬기/데미지 애니메이션 구현
- BattleEntity: OnDamaged 이벤트 추가

## 진행 중인 작업 (In Progress)
- Unity에서 UI 프리팹 구성
  - StatusEffectSlotUI 프리팹 생성 (Image + Text "Count")
  - PlayerInfomation, EnemyInfomation에 슬롯 할당

## 대기 중인 작업 (Pending)
- 던전 시스템 구현

## 폴더 구조

```
Assets/Scripts/
├── Core/
│   ├── GameInstaller.cs        ← DI 서비스 등록
│   ├── Services.cs             ← 서비스 로케이터
│   ├── IAssetLoader.cs         ← Addressables 로더 인터페이스
│   ├── AddressablesLoader.cs
│   ├── IGameDataManager.cs     ← 데이터 매니저 인터페이스
│   └── GameDataManager.cs
│
├── Character/
│   ├── Player.cs               ← List<InventoryItem> 20칸
│   ├── Monster.cs
│   ├── BattleEntity.cs         ← 버프/상태이상 관리
│   ├── ActiveBuff.cs           ← GetIconAddress() 추가
│   └── IBattleEntity.cs        ← ActiveStatusEffect 정의
│
├── Items/
│   └── InventoryItem.cs        ← Equipment/Consumable 래퍼, Rarity
│
├── ScriptableObjects/
│   ├── MonsterDataSO.cs        ← PrefabAddress
│   ├── SkillDataSO.cs          ← IconAddress, Cooldown
│   ├── EquipmentDataSO.cs      ← IconAddress
│   ├── ConsumableDataSO.cs     ← IconAddress
│   ├── RarityDataSO.cs         ← Color 필드
│   └── StatusEffectDataSO.cs   ← IconAddress 필드
│
├── Skill/
│   ├── ISkillManager.cs
│   └── SkillManager.cs         ← 스킬 풀, 쿨다운, 실행
│
├── Combat/
│   ├── BattleManager.cs
│   ├── BattleController.cs
│   ├── DamageCalculator.cs
│   └── TurnManager.cs
│
├── UI/Battle/
│   ├── BattleUI.cs
│   ├── PlayerStatusUI.cs       ← _debuffSlots, _buffSlots
│   ├── MonsterStatusUI.cs      ← _debuffSlots
│   ├── StatusEffectSlotUI.cs   ← 아이콘 + Count 텍스트
│   ├── SkillSlotUI.cs          ← 쿨다운 표시
│   ├── ActionMenuUI.cs
│   └── BattleLogUI.cs
│
├── UI/Inventory/
│   ├── InventoryUI.cs
│   ├── InventorySlotUI.cs
│   ├── EquipSlotUI.cs
│   ├── ItemTooltipUI.cs
│   └── ConfirmDropPopup.cs
│
├── Editor/
│   ├── CSVConverter.cs         ← CSV → ScriptableObject
│   └── ScriptableObjectAddressablesSetter.cs ← 아이콘 Addressables 등록
│
└── Tests/
    └── AddressablesTest.cs

Assets/Prefabs/
├── Monster/                    ← 몬스터 프리팹 (5개)
├── SkillIcon/                  ← 스킬 아이콘 Sprite (15개)
├── Elementer/                  ← 속성 아이콘 (Fire, Water, Leaf, Neutral)
└── UI/                         ← UI 프리팹

Assets/ScriptableObjects/Data/
├── Monsters/ (5개)
├── Skills/ (15개)
├── Equipments/ (15개)
├── Consumables/ (10개)
├── Rarities/ (5개)
└── StatusEffects/ (3개)

Assets/EditorData/Data/csv/
├── MonsterData.csv
├── SkillData.csv
├── EquipmentData.csv
├── ConsumableData.csv
├── RarityData.csv
└── StatusEffect.csv           ← IconAddress 컬럼 포함
```

## 아이콘 주소 매핑

| 타입 | 주소 |
|-----|------|
| 버프 - 공격력 | Skills/PowerBuff |
| 버프 - 방어력 | Skills/DefenseBuff |
| 버프 - 속도 | Skills/SpeedBuff |
| 디버프 - 화상 | Skills/BurnDeBuff |
| 디버프 - 독 | Skills/PoisonDeBuff |
| 디버프 - 스턴 | Skills/Stun |
| 속성 - 불 | Elements/Fire |
| 속성 - 물 | Elements/Water |
| 속성 - 풀 | Elements/Leaf |
| 속성 - 없음 | Elements/Neutral |

---
최종 업데이트: 2026-03-23

## 커밋 기록

| 날짜 | 커밋 | 내용 |
|------|------|------|
| 2026-03-23 | 21630bb | StatusEffectSlotUI 추가, 버프/디버프 슬롯 풀 시스템 구현 |
| 2026-03-23 | - | 스킬 시스템, 속성 아이콘, StatusEffect IconAddress 추가 |
| 2026-03-20 | ad63a6d | MonsterSpriteView 애니메이션 시스템 구현 |