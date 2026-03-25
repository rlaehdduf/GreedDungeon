# 전투 시스템 비주얼 개편 설계

## 개요

전투 시스템에 시각적 효과와 애니메이션을 추가하여 전투 경험을 개선한다.

## 요구사항

1. 각 턴마다 1~1.5초 딜레이 추가
2. 적 공격 시 스케일 확대 모션
3. 데미지 수치 텍스트 표시 (Canvas 내부 랜덤 위치)
4. 공격 이펙트 (Neutral, Melee, Magic) 스프라이트
5. 디버프 시각 효과 (화면 테두리 그라데이션)

---

## 1. 전투 흐름 제어 (TurnDelaySystem)

### 목적
전투가 너무 빠르게 진행되는 문제를 해결하기 위해 각 액션 사이에 딜레이 추가

### 구현
- `BattleManager`에 코루틴 기반 딜레이 시스템 추가
- 각 액션(공격, 스킬, 아이템) 실행 후 1~1.5초 대기
- `BattleController`에서 시퀀스 관리

### 흐름
```
플레이어 공격 → 0.3초 대기 → 모션/이펙트 → 0.5초 대기 → 데미지 적용 → 0.5초 대기 → 턴 종료
```

### 설정값
| 액션 | 딜레이 |
|-----|-------|
| 공격 시작 | 0.3초 |
| 이펙트 표시 | 0.5초 |
| 데미지 적용 후 | 0.5초 |
| 턴 전환 | 0.3초 |

---

## 2. 적 공격 모션 (MonsterAnimation)

### 목적
적이 공격할 때 시각적 피드백 제공

### 구현
- `MonsterDisplay`에 스케일 애니메이션 추가
- 공격 시 스케일 1.3~1.5배 확대 후 원래 크기로 복귀
- 코루틴으로 0.3~0.5초간 실행

### 코드 위치
- `Assets/Scripts/UI/Battle/MonsterDisplay.cs` 수정

### 애니메이션 파라미터
| 파라미터 | 값 |
|---------|---|
| 확대 배율 | 1.3x ~ 1.5x |
| 지속 시간 | 0.4초 |
| 이징 | Ease Out Quad |

---

## 3. 데미지 텍스트 표시 (DamageTextUI)

### 목적
공격 시 데미지 수치를 시각적으로 표시

### 구현
- Canvas 내부에 `DamageTextContainer` 생성
- 텍스트가 랜덤 위치에서 위로 튀어올랐다가 페이드 아웃
- 오브젝트 풀링으로 성능 최적화

### 컴포넌트 구조
```
Canvas
└── DamageTextContainer
    ├── DamageText (prefab)
    │   └── Text ("-12")
    └── DamageTextPool (component)
```

### 애니메이션
1. 랜덤 위치에 생성
2. 위로 이동 (0.5초)
3. 페이드 아웃 (0.3초)
4. 풀로 반환

### 색상
| 타입 | 색상 |
|-----|-----|
| 일반 데미지 | 흰색 |
| 크리티컬 | 노란색 |
| 회복 | 녹색 |

---

## 4. 공격 이펙트 (AttackEffectUI)

### 목적
공격 타입에 따른 시각적 이펙트 표시

### 구현
- Canvas에 `EffectContainer` 추가
- 스킬 타입별 스프라이트 로드 (Addressables)
- 이펙트 표시 후 0.5초 후 페이드 아웃

### 이펙트 타입
| 타입 | 설명 |
|-----|-----|
| Neutral | 기본 물리 공격 |
| Melee | 근접 무기 공격 |
| Magic | 마법 공격 |

### 리소스
- `Assets/Sprites/Effects/Neutral.png`
- `Assets/Sprites/Effects/Melee.png`
- `Assets/Sprites/Effects/Magic.png`
- Addressables 주소: `Effects/Neutral`, `Effects/Melee`, `Effects/Magic`

---

## 5. 디버프 시각 효과 (DebuffVisualEffect)

### 목적
디버프 상태를 시각적으로 표현

### Player 디버프 (화면 테두리 그라데이션)

#### 구현
- Canvas에 `DebuffVignetteUI` 추가
- 화면 전체를 덮는 이미지 (가장자리 불투명, 중앙 투명)
- 디버프 타입에 따라 색상 변경

#### 컴포넌트 구조
```
Canvas
└── DebuffVignette (Image)
    ├── Sprite: Radial Gradient
    └── Color: 디버프별 색상
```

#### 색상 매핑
| 디버프 | 색상 | Hex |
|-------|-----|-----|
| Burn | 빨간색 | #FF4444 |
| Poison | 녹색 | #44FF44 |
| Stun | 노란색 | #FFFF44 |

#### 동작
- 디버프 적용 시: 해당 색상으로 페이드 인 (0.5초)
- 디버프 종료 시: 페이드 아웃 (0.5초)
- 여러 디버프 동시 적용 시: 가장 최근 디버프 색상 표시

### Monster 디버프 (색상 오버레이)

#### 구현
- `MonsterDisplay`의 SpriteRenderer.color 변경
- 디버프 타입에 따라 색상 틴트 적용

---

## 파일 구조

```
Assets/Scripts/
├── Combat/
│   └── BattleManager.cs (수정) - 딜레이 시스템 추가
│
├── UI/Battle/
│   ├── MonsterDisplay.cs (수정) - 공격 모션, 디버프 색상
│   ├── BattleUI.cs (수정) - 이펙트/데미지 텍스트 연결
│   ├── DamageTextUI.cs (신규) - 데미지 텍스트 풀
│   ├── AttackEffectUI.cs (신규) - 공격 이펙트
│   └── DebuffVignetteUI.cs (신규) - 화면 테두리 그라데이션
│
└── Prefabs/
    └── UI/
        ├── DamageText.prefab (신규)
        └── AttackEffect.prefab (신규)
```

---

## 구현 순서

1. **턴 딜레이 시스템** - BattleManager 수정
2. **적 공격 모션** - MonsterDisplay 수정
3. **데미지 텍스트** - DamageTextUI 신규 생성
4. **공격 이펙트** - AttackEffectUI 신규 생성
5. **디버프 시각 효과** - DebuffVignetteUI 신규 생성

---

## 테스트 항목

- [ ] 각 턴 사이 딜레이 정상 작동
- [ ] 적 공격 모션 정상 표시
- [ ] 데미지 텍스트 정상 표시 및 애니메이션
- [ ] 공격 이펙트 정상 표시
- [ ] 디버프 시 화면 테두리 색상 변경
- [ ] 디버프 종료 시 테두리 페이드 아웃