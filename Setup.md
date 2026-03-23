# Unity 수동 설정 가이드

## 필수 설정 (1회성)

### 1. Build Settings
- **File → Build Settings** → 씬 추가: `Title`, `Dungeon`, `Battle`

### 2. Title 씬 DI 설정
- `GameRoot` GameObject 생성
- `RootContext` + `GameInstaller` 컴포넌트 추가
- `RootContext.Installers`에 `GameInstaller` 할당

### 3. CSV 변환
- **Tools → CSV → Convert All**

### 4. Addressables 설정
- **Tools → Addressables → Set All Prefab Addresses**
- ScriptableObject 라벨 설정: `MonsterData`, `SkillData`, `EquipmentData`, `ConsumableData`, `RarityData`, `StatusEffectData`
- **Window → Asset Management → Addressables → Groups → Build → New Build**

---

## Battle 씬 설정

### MonsterDisplay
1. 빈 GameObject 생성 (이름: `MonsterDisplay`)
2. 컴포넌트 추가: `MonsterDisplay`, `MonsterSpriteView`, `SpriteRenderer`
3. 인스펙터 연결:
   - `_spriteView` → MonsterSpriteView
   - `_spriteRenderer` → SpriteRenderer

### BattleController
1. 빈 GameObject 생성 (이름: `BattleController`)
2. `BattleController` 컴포넌트 추가
3. `_monsterDisplay` → MonsterDisplay 연결
4. 테스트 버튼: OnClick → `BattleController.StartTestBattle()`

---

## 확인 체크리스트

- [ ] Build Settings에 3개 씬 추가
- [ ] Title 씬 DI 설정
- [ ] CSV 변환 완료
- [ ] Addressables 주소/라벨 설정 및 빌드
- [ ] Battle 씬: MonsterDisplay, BattleController 설정