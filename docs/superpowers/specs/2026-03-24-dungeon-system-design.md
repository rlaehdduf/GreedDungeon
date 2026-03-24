# 던전 시스템 설계

## 개요

Battle 씬에서 지속적인 전투 진행. 몬스터 처치 후 보상 선택 → 배경 스크롤 → 다음 조우. 일정 처치 수 이후 보스 등장.

## 핵심 기능

### 1. 전투 루프
- 기본 전투 화면 유지
- 몬스터 처치 → 보상 확인 → 배경 스크롤 → 다음 조우

### 2. 조우 시스템
- 전투 60%, 보물 25%, 상점 15%
- 전투 선택 시 보스 확률 체크

### 3. 보스 확률
- 10마리 처치 전: 보스 0%
- 10마리 처치 후: (처치 수 - 9) × 10%
- 예: 10마리=10%, 11마리=20%, ... 19마리=100%

### 4. 보물
- 골드 + 아이템 동시 획득
- UI 팝업으로 표시

### 5. 상점
- 인벤토리 UI 재사용 (슬롯 3개)
- ItemTooltipUI에 가격 표시
- 구매/판매 모두 지원

### 6. 배경 스크롤
- transform.localScale Y축 조절
- 위아래 움직임 연출

## 아키텍처

```
Assets/Scripts/Dungeon/
├── DungeonController.cs      # 전체 흐름 제어
├── DungeonState.cs           # 상태 enum
├── DungeonProgress.cs        # 진행도 (처치 수, 보스)
├── EncounterSystem.cs        # 조우 확률 계산
├── BackgroundScroller.cs     # 배경 스크롤 애니메이션
└── UI/
    ├── TreasurePopupUI.cs    # 보물 획득 팝업
    └── ShopUI.cs             # 상점 UI
```

## 데이터 흐름

```
게임 시작
    ↓
[DungeonController] 초기화
    ↓
[BattleController] 전투 → 몬스터 처치
    ↓
[DungeonProgress] 처치 수 증가
    ↓
[EncounterSystem] 조우 결정
    ├─ 보물 (25%) → [TreasurePopupUI] → 골드+아이템
    ├─ 상점 (15%) → [ShopUI] → 구매/판매
    └─ 전투 (60%) → 보스 확률 체크
         ├─ killCount < 10: 보스 0%
         └─ killCount >= 10: 보스 확률 = (killCount - 9) × 10%
              ├─ 보스 당첨 → Boss 전투
              └─ 보스 미당첨 → 일반 전투
    ↓
[BackgroundScroller] → 다음 전투
```

## 기존 코드 연동

### BattleController
- `OnBattleEnded` 이벤트 추가

### BattleUI
- 상태별 UI 활성화

### InventoryUI
- ShopUI에서 슬롯 프리팹 재사용

## 컴포넌트 상세

### DungeonState
```csharp
public enum DungeonState
{
    Battle,
    Moving,
    Treasure,
    Shop,
    Boss
}
```

### DungeonProgress
- `KillCount` - 처치 수
- `IsBossDefeated` - 보스 처치 여부
- `GetBossProbability()` - 보스 확률 계산

### EncounterSystem
- `GetNextEncounter()` - 조우 타입 반환
- 확률 계산 로직

### BackgroundScroller
- 스케일 Y축 조절
- 애니메이션 완료 콜백

### TreasurePopupUI
- 골드 표시
- 아이템 표시
- 획득 버튼

### ShopUI
- InventorySlotUI 3개
- ItemTooltipUI (이름 + 가격)
- 구매/판매 기능

## 테스트 시나리오

1. 기본 전투 루프 (10마리 처치)
2. 보물 조우 (골드 + 아이템)
3. 상점 조우 (구매/판매)
4. 보스 확률 증가 (10마리 이후)
5. 보스 등장 및 처치