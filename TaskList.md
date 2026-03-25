# Unity 작업 목록

---

## 전투 비주얼 시스템 설정 (2026-03-25)

### 1. 데미지 텍스트 컨테이너

**위치:** Canvas 하위

**생성:**
1. 빈 GameObject 생성 → 이름: `DamageTextContainer`
2. `DamageTextUI` 컴포넌트 추가
3. BattleUI의 `_damageTextUI` 필드에 연결

**설정:**
- RectTransform: 화면 중앙 또는 적당한 위치
- 인스펙터에서 `_poolSize`, `_moveUpDistance`, `_animationDuration` 조절 가능

---

### 2. 공격 이펙트 컨테이너

**위치:** Canvas 하위

**생성:**
1. UI → Image 생성 → 이름: `AttackEffect`
2. `AttackEffectUI` 컴포넌트 추가
3. BattleUI의 `_attackEffectUI` 필드에 연결

**설정:**
- RectTransform: 화면 중앙, 크기 200x200 정도
- Color: Alpha 0 (초기에는 안 보이게)
- Raycast Target: 해제

---

### 3. 디버프 비네트 (화면 테두리 그라데이션)

**위치:** Canvas 하위 (가장 아래)

**생성:**
1. UI → Image 생성 → 이름: `DebuffVignette`
2. 화면 전체 크기로 설정 (Anchor: Stretch/Stretch, Left/Top/Right/Bottom = 0)
3. `DebuffVignetteUI` 컴포넌트 추가
4. BattleUI의 `_debuffVignetteUI` 필드에 연결

**스프라이트 (필수):**
- 가장자리는 불투명, 중앙은 투명한 원형 그라데이션 이미지 필요
- Photoshop/GIMP에서 제작:
  - 새 이미지 (512x512)
  - Radial Gradient (검은색 → 투명)
  - 저장: `Assets/Sprites/UI/Vignette.png`

**설정:**
- Color: Alpha 0 (초기에는 안 보이게)
- Raycast Target: 해제

---

### 4. 이펙트 스프라이트 Addressables 등록

**폴더 생성:**
```
Assets/Sprites/Effects/
├── Neutral.png
├── Melee.png
└── Magic.png
```

**등록 방법:**
1. 각 스프라이트 선택
2. Inspector → Addressable 체크
3. Address 설정:
   - `Neutral.png` → `Effects/Neutral`
   - `Melee.png` → `Effects/Melee`
   - `Magic.png` → `Effects/Magic`

**빌드:**
- Tools → Addressables → 🔄 Setup & Build

**⚠️ 주의: Addressables 확인 방법**
1. Window → Asset Management → Addressables → Groups
2. 검색창에서 각 주소 검색:
   - `Effects/Neutral` → 올바른 스프라이트인지 확인
   - `Effects/Melee` → 올바른 스프라이트인지 확인
   - `Effects/Magic` → 올바른 스프라이트인지 확인
3. 잘못되었으면 스프라이트 선택 → Inspector에서 Address 수정
4. 수정 후 **반드시 다시 빌드**

---

### 4-1. 디버프 아이콘 Addressables 등록

**이미 등록된 주소 (CONTEXT.md 참고):**
| 디버프 | Address |
|-------|---------|
| Burn | Skills/BurnDeBuff |
| Poison | Skills/PoisonDeBuff |
| Stun | Skills/Stun |

**⚠️ Stun 아이콘이 없을 경우:**
1. `Assets/Sprites/Skills/` 폴더에 Stun 아이콘 스프라이트 배치
2. 스프라이트 선택 → Inspector → Addressable 체크
3. Address: `Skills/Stun`
4. Tools → Addressables → 🔄 Setup & Build

---

### 5. BattleUI 컴포넌트 연결

**Hierarchy:** Canvas 선택 → BattleUI 컴포넌트

**Inspector에서 연결:**
| 필드 | 연결 대상 |
|-----|----------|
| `_damageTextUI` | DamageTextContainer |
| `_attackEffectUI` | AttackEffect |
| `_debuffVignetteUI` | DebuffVignette |

---

### 6. 턴 딜레이 설정 (선택)

**Hierarchy:** Managers → BattleController 선택

**Inspector에서 조절:**
| 필드 | 기본값 | 설명 |
|-----|-------|------|
| `_attackStartDelay` | 0.3초 | 공격 시작 전 대기 |
| `_effectDisplayDelay` | 0.5초 | 이펙트 표시 후 대기 |
| `_afterDamageDelay` | 0.5초 | 데미지 적용 후 대기 |
| `_turnTransitionDelay` | 0.3초 | 턴 전환 대기 |

---

## 디버프 색상 매핑

| 디버프 | ID | 화면 테두리 색상 | 몬스터 오버레이 |
|-------|---|-----------------|----------------|
| Burn | 1 | 빨간색 (#FF4444) | 빨간색 틴트 |
| Poison | 2 | 녹색 (#44FF44) | 녹색 틴트 |
| Stun | 3 | 노란색 (#FFFF44) | 노란색 틴트 |

---

## 완료 체크리스트

- [ ] DamageTextContainer 생성 및 연결
- [ ] AttackEffect 생성 및 연결
- [ ] DebuffVignette 생성 및 연결
- [ ] 비네트 스프라이트 제작
- [ ] 이펙트 스프라이트 3종 준비
- [ ] Addressables 등록 및 빌드
- [ ] 테스트: 공격 시 데미지 텍스트 표시
- [ ] 테스트: 디버프 시 화면 테두리 색상 변경