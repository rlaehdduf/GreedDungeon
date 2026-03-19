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

## 3. 각 씬 저장

Unity 에디터에서 각 씬을 열어서 저장:
- `Title.unity`
- `Dungeon.unity`
- `Battle.unity`

## 4. 확인 사항

- [ ] Build Settings에 3개 씬 추가됨
- [ ] Title 씬에 RootContext + GameInstaller 설정됨
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
| Unity 수동 설정 | 대기 중 |