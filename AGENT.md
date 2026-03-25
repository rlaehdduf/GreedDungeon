# AGENT.md - AI 에이전트 커맨드 가이드

## 빠른 참조

| 필요한 정보 | 파일 |
|------------|------|
| 현재 작업 상태 | `Context.md` |
| Unity 수동 설정 | `docs/setup/Setup.md` |
| 게임 설계 문서 | `docs/design/게임개발계획서.md` |
| 변경 이력 | `docs/history/` |

## 커맨드

| 커맨드 | 설명 |
|--------|------|
| `/check` | 현재 작업 상태 확인 |
| `/setup` | Unity 수동 설정 가이드 |
| `/design` | 게임 설계 문서 |
| `/history` | 리팩토링/변경 이력 |

## 개발 규칙

1. **항상 물어보고 진행** - 작업 전 사용자 승인 필수
2. **Pathfinder Core DI** - MonoBehaviour는 `Services.Get<T>()` 사용
3. **SOLID 준수** - 클래스 300행/메서드 10개 제한
4. **CSV 워크플로우**: `GameData.xlsm` → CSV → `CSVConverter` → ScriptableObject

## 주요 스킬

| 스킬 | 설명 |
|------|------|
| `.opencode/skills/di-troubleshooting.md` | Pathfinder DI 문제 해결 |
| `.opencode/skills/commit-and-update-progress.md` | 커밋 및 Progress 저장 |

## 중요 발견 사항

1. **순환 의존성** - `Services.Get<T>()`로 해결
2. **비동기 초기화** - `EnsureInitialized()` 지연 초기화
3. **Unity 코루틴** - `yield return Task`는 대기하지 않음
4. **툴팁 Raycast** - `CanvasGroup.blocksRaycasts = false`
5. **Addressables** - Sprite 폴더에 아이콘 배치 후 등록
6. **Input System** - `UnityEngine.InputSystem` 사용

---

## Setup 문서 관리 규칙

Setup.md에 새 내용 추가 시 다음 규칙을 따른다:

### 1. 기존 내용 아카이브
```bash
# 기존 Setup.md 내용을 Setup_archive.md로 이동
# 새 내용만 Setup.md에 작성
```

### 2. 아카이브 분리 (500줄 초과 시)
```
docs/setup/
├── Setup.md           # 현재 작업 (최신)
├── Setup_archive.md   # 아카이브 1 (500줄 이하)
├── Setup_archive_1.md # 아카이브 2 (500줄 초과 시 생성)
└── Setup_archive_2.md # 아카이브 3 ...
```

### 3. 워크플로우
1. Setup_archive.md 줄 수 확인
2. 500줄 이상이면 `Setup_archive_1.md` 생성 (기존 archive 내용 이동)
3. 기존 Setup.md 내용을 `Setup_archive.md`로 이동
4. 새 내용을 Setup.md에 작성

### 예시
```bash
# 1. 아카이브 줄 수 체크
wc -l docs/setup/Setup_archive.md

# 2. 500줄 초과 시 새 아카이브 생성
# Setup_archive.md → Setup_archive_1.md
# Setup_archive.md (비우고 Setup.md 기존 내용 이동)

# 3. Setup.md에 새 내용 작성
```