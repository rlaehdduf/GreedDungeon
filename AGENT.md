# AGENT.md - GreedDungeon 프로젝트 가이드

## 프로젝트 개요
Unity 2D 던전 크롤러 턴제 RPG 프로토타입

## 주요 스킬

| 스킬 | 설명 |
|------|------|
| `di-troubleshooting.md` | Pathfinder DI 문제 해결 가이드 |
| `commit-and-update-progress.md` | 커밋 및 Progress.md 저장 |

## 개발 규칙

### 1. 작업 진행
- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**

### 2. 코드 스타일
- Pathfinder Core DI 시스템 사용
- SOLID 준수 (클래스 300행/메서드 10개 제한)
- 서비스 로케이터 패턴: `Services.Get<T>()` 사용

### 3. 데이터 워크플로우
```
GameData.xlsm → 자동 CSV 저장 → CSVConverter → ScriptableObject
```

### 4. Addressables 구조
- **프리팹 주소**: `Monsters/Slime`, `Weapons/Sword`, `Skills/Smash`
- **SO 주소**: 프리팹과 동일 (예: `Weapons/Sword`)
- **라벨**: `MonsterData`, `SkillData`, `EquipmentData`, `ConsumableData`, `RarityData`, `StatusEffectData`

## 폴더 구조

```
Assets/
├── Scripts/
│   ├── Core/           # 핵심 시스템
│   ├── ScriptableObjects/  # 데이터 정의
│   ├── Editor/         # 에디터 도구
│   └── Tests/          # 테스트 코드
├── ScriptableObjects/Data/  # 변환된 데이터
├── Prefabs/            # 프리팹
└── EditorData/Data/csv/  # CSV 원본
```

## 발견 사항 (중요)

1. **Unity CLI 연결 문제** - HTTP server는 실행되지만 명령이 타임아웃됨
2. **CSV 파싱 UTF-8 BOM 문제** - `File.ReadAllBytes()`로 직접 읽고 수동 디코딩
3. **생성자 순서 버그** - `InitializeStats()` 패턴으로 해결
4. **Resources 폴더 보안 이슈** - EditorData 폴더로 이동하여 해결
5. **Pathfinder DI [Inject] 제한** - MonoBehaviour는 `Services.Get<T>()` 사용

## 현재 작업 상태

진행 중인 작업과 완료된 작업은 `Progress.md`에서 확인하세요.

## 관련 파일

- `Progress.md` - 진행상황
- `Setup.md` - Unity 수동 설정
- `.opencode/skills/di-troubleshooting.md` - DI 문제 해결
- `.opencode/skills/commit-and-update-progress.md` - 커밋 및 저장