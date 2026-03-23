---
## Goal

던전 크롤러 턴제 RPG 프로토타입 개발 (Unity 2D)

**현재 작업:** Unity에서 UI 프리팹 구성 (StatusEffectSlotUI 슬롯 할당)

## Instructions

- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**
- **Pathfinder Core DI 시스템 사용** - MonoBehaviour는 `Services.Get<T>()` 사용
- **SOLID 준수** - 클래스 300행/메서드 10개 제한
- **순환 의존성 주의** - DI 생성자 주입 대신 `Services.Get<T>()` 런타임 호출로 해결

## Discoveries

1. **순환 의존성** - BattleManager ↔ SkillManager → `Services.Get<T>()`로 해결
2. **비동기 초기화 순서** - `EnsureInitialized()` 지연 초기화 패턴
3. **Unity 코루틴과 Task** - `yield return Task`는 대기하지 않음 → `IsInitialized` 폴링
4. **툴팁 Raycast 차단** - CanvasGroup `blocksRaycasts = false`
5. **Addressables 타입** - Sprite 폴더에 아이콘 배치 후 Addressables 등록

## Accomplished

- Phase 1~4 (전투, 속성/상태이상, 인벤토리/버프) ✅
- Phase 5 스킬 시스템 + 스킬 슬롯 UI + 툴팁 ✅
- 인벤토리 UI 시스템 (InventoryItem, InventorySlotUI, EquipSlotUI, ItemTooltipUI, ConfirmDropPopup) ✅
- 버프/디버프 아이콘 시스템 (StatusEffectSlotUI, Addressables 로드) ✅
- 데이터 워크플로우 (CSV → ScriptableObject, VBA 매크로) ✅

## In Progress

- Unity UI 프리팹 구성 (StatusEffectSlotUI 슬롯 할당)

## Pending

- 던전 시스템

## Key Files

```
Assets/Scripts/
├── UI/Battle/
│   ├── StatusEffectSlotUI.cs   ← 아이콘 + Count 텍스트
│   ├── PlayerStatusUI.cs       ← _debuffSlots, _buffSlots
│   └── MonsterStatusUI.cs      ← _debuffSlots
├── Character/
│   ├── ActiveBuff.cs           ← GetIconAddress()
│   └── IBattleEntity.cs        ← ActiveStatusEffect
└── ScriptableObjects/
    └── StatusEffectDataSO.cs   ← IconAddress

Assets/EditorData/Data/csv/
└── StatusEffect.csv            ← IconAddress 컬럼
```

## Icon Addresses

| 타입 | 주소 |
|-----|------|
| 버프 | Skills/PowerBuff, Skills/DefenseBuff, Skills/SpeedBuff |
| 디버프 | Skills/BurnDeBuff, Skills/PoisonDeBuff, Skills/Stun |
| 속성 | Elements/Fire, Elements/Water, Elements/Leaf, Elements/Neutral |

---