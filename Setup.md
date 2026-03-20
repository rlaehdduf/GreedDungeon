# Unity 수동 설정 가이드

## 1. Build Settings에 씬 추가

1. **File → Build Settings** 열기
2. **Scenes In Build**에 다음 씬들을 드래그:
   - `Assets/Scenes/Title.unity`
   - `Assets/Scenes/Dungeon.unity`
   - `Assets/Scenes/Battle.unity`
3. **Title** 씬이 인덱스 0이 되도록 순서 조정

## 2. Title 씬에 DI 설정

1. **Title.unity** 씬 열기
2. 빈 GameObject 생성 (이름: `GameRoot`)
3. `RootContext` 컴포넌트 추가
4. `GameInstaller` 컴포넌트 추가
5. `RootContext`의 Installers 리스트에 `GameInstaller` 할당

## 3. CSV → ScriptableObject 변환

1. Unity 메뉴: **Tools → CSV → Convert All**
2. 변환 결과 확인:
   - `Assets/ScriptableObjects/Data/StatusEffects/`
   - `Assets/ScriptableObjects/Data/Rarities/`
   - `Assets/ScriptableObjects/Data/Skills/`
   - `Assets/ScriptableObjects/Data/Equipments/`
   - `Assets/ScriptableObjects/Data/Monsters/`

### 개별 변환 메뉴

- `Tools/CSV/Convert StatusEffects`
- `Tools/CSV/Convert Rarities`
- `Tools/CSV/Convert Skills`
- `Tools/CSV/Convert Equipments`
- `Tools/CSV/Convert Monsters`

## 4. 확인 사항

- [ ] Build Settings에 3개 씬 추가됨
- [ ] Title 씬에 RootContext + GameInstaller 설정됨
- [ ] CSV 변환 완료
- [ ] ScriptableObject 생성됨
- [ ] 콘솔에 에러 없음

---

## 진행 상황

| 작업 | 상태 |
|------|------|
| 폴더 구조 생성 | 완료 |
| 씬 생성 (Title, Dungeon, Battle) | 완료 |
| GameInstaller.cs | 완료 |
| IGameManager.cs | 완료 |
| ISceneLoader.cs | 완료 |
| GameManager.cs | 완료 |
| SceneLoader.cs | 완료 |
| Stats.cs | 완료 |
| ScriptableObject 5개 | 완료 |
| CSVConverter 에디터 툴 | 완료 |
| Unity 수동 설정 | 대기 중 |
| CSV 변환 실행 | 대기 중 |

---

## 데이터 구조

### CSV 파일 위치
```
Assets/Resources/Data/
├── MonsterData.csv
├── EquipmentData.csv
├── RarityData.csv
├── SkillData.csv
└── StatusEffect.csv
```

### ScriptableObject 출력 위치
```
Assets/ScriptableObjects/Data/
├── Monsters/
├── Equipments/
├── Rarities/
├── Skills/
└── StatusEffects/
```

---

## 참고: CSV 오타 자동 수정

CSVConverter에서 자동 수정:
- `Fasle` → `FALSE`
- `Brun` → `Burn`
- `Dagame` → `Damage`

---

## 수정 내역

### 2026-03-19: CSVConverter 빈 데이터 처리
- `ReadCSV()` 메서드에 빈 줄 체크 추가 (`string.IsNullOrWhiteSpace`)
- 빈 필드 체크 추가 (ID가 비어있으면 스킵)
- CSV 끝에 빈 줄이 있어도 에러 발생하지 않음

### 2026-03-20: Addressables 런타임 로드 시스템
- `IAssetLoader.cs`, `AddressablesLoader.cs` 추가
- `IGameDataManager.cs`, `GameDataManager.cs` 추가
- `GameInstaller.cs`에 새 서비스 등록

---

## 5. Addressables 설정 (필수)

### 5.1 프리팹 주소 설정

1. Unity 메뉴: **Tools → Addressables → Set All Prefab Addresses**
2. 콘솔에서 "Addressables 주소 설정 완료! 총 XX개" 확인
3. 확인: **Window → Asset Management → Addressables → Groups**
   - 각 프리팹에 커스텀 주소가 설정되어 있어야 함
   - 예: `Monsters/Slime`, `Skills/Smash`, `Weapons/Sword`

### 5.2 ScriptableObject 데이터 라벨 설정

GameDataManager가 데이터를 로드하려면 라벨이 필요합니다:

1. **Window → Asset Management → Addressables → Groups** 열기
2. 각 ScriptableObject 그룹 선택 후 Labels 추가:
   - `Assets/ScriptableObjects/Data/Monsters/` → Label: `MonsterData`
   - `Assets/ScriptableObjects/Data/Skills/` → Label: `SkillData`
   - `Assets/ScriptableObjects/Data/Equipments/` → Label: `EquipmentData`
   - `Assets/ScriptableObjects/Data/Consumables/` → Label: `ConsumableData`
   - `Assets/ScriptableObjects/Data/Rarities/` → Label: `RarityData`
   - `Assets/ScriptableObjects/Data/StatusEffects/` → Label: `StatusEffectData`

### 5.3 Addressables 빌드

1. **Window → Asset Management → Addressables → Groups**
2. **Build → New Build → Default Build Script** 실행
3. 빌드 완료 대기

---

## 6. 테스트 씬 설정

### 6.1 테스트 씬 생성

1. `Assets/Scenes/AddressablesTest.unity` 생성
2. Build Settings에 추가
3. 씬에 설정:
   - 빈 GameObject (이름: `GameRoot`)
   - `RootContext` 컴포넌트 추가
   - `GameInstaller` 컴포넌트 추가
   - `RootContext`의 Installers 리스트에 `GameInstaller` 할당
   - `AddressablesTest` 컴포넌트 추가

### 6.2 테스트 UI 구성

AddressablesTest 컴포넌트에 할당:
- `LoadMonsterButton` - 몬스터 프리팹 로드 테스트
- `LoadSkillButton` - 스킬 아이콘 로드 테스트
- `LoadAllDataButton` - 모든 데이터 목록 출력
- `SpawnPoint` - 프리팹 생성 위치 (Transform)
- `StatusText` - 상태 메시지 표시 (Text)

---

## 확인 사항 (업데이트)

- [ ] Build Settings에 3개 씬 추가됨
- [ ] Title 씬에 RootContext + GameInstaller 설정됨
- [ ] CSV 변환 완료
- [ ] ScriptableObject 생성됨
- [ ] Addressables 프리팹 주소 설정됨
- [ ] ScriptableObject 라벨 설정됨
- [ ] Addressables 빌드 완료
- [ ] 테스트 씬에서 로드 테스트 성공