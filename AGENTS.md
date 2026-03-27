# AGENTS.md - AI 에이전트 가이드

Unity 게임 개발 시 AI 에이전트가 참고할 가이드입니다.

---

## 개발 규칙

| 규칙 | 설명 |
|------|------|
| 승인 필수 | 작업 전 사용자 승인 받기 |
| SOLID 준수 | 클래스 300행 / 메서드 10개 제한 |
| 한글 주석 | 코드 주석은 한글로 작성 |
| DI 사용 | `Services.Get<T>()`로 서비스 접근 |

---

## 커맨드

| 커맨드 | 설명 |
|--------|------|
| `/check` | 현재 작업 상태 확인 (`Context.md`) |
| `/setup` | Unity 수동 설정 가이드 (`docs/setup/Setup.md`) |
| `/design` | 게임 설계 문서 |
| `/changelog` | GameDevToolkit 변경 이력 (`CHANGELOG.md`) |

---

## 패턴 & 해결책

### 1. 순환 의존성

```
A → B → A (순환)
```

**해결:** `Services.Get<T>()` 지연 해결

```csharp
// Bad: 생성자에서 직접 참조
public class BattleManager
{
    private SkillManager _skillManager;
    public BattleManager(SkillManager sm) => _skillManager = sm;
}

// Good: Services로 지연 참조
public class BattleManager
{
    private ISkillManager _skillManager;
    private void Init() => _skillManager = Services.Get<ISkillManager>();
}
```

### 2. 비동기 초기화 순서

**문제:** 서비스 초기화 전에 접근

**해결:** `EnsureInitialized()` 패턴

```csharp
public async Task EnsureInitialized()
{
    while (!Services.IsInitialized)
    {
        await Task.Delay(100);
    }
}
```

### 3. Unity 코루틴과 Task

**문제:** `yield return Task`는 대기하지 않음

```csharp
// Bad - 대기 안함
yield return SomeAsyncTask();

// Good - 폴링
yield return new WaitUntil(() => IsDataLoaded);
```

### 4. 툴팁 Raycast 차단

```csharp
// 툴팁이 클릭을 막지 않도록
_canvasGroup.blocksRaycasts = false;
```

### 5. 정적 캐싱 (초기화 순서 문제)

```csharp
// 생성자에서 Services 미초기화 가능
private static PlayerDataSO _cachedData;

private static PlayerDataSO GetData()
{
    if (_cachedData != null) return _cachedData;
    
    if (Services.IsInitialized)
    {
        _cachedData = Services.Get<IGameDataManager>().GetPlayerData();
    }
    return _cachedData;
}
```

---

## CSV 워크플로우

```
GameData.xlsm (Excel)
      ↓ 저장 시 자동
    CSV 파일
      ↓ Unity 실행 시 자동
ScriptableObject
      ↓
  Addressables
```

**자동 변환 설정:**
- `Tools > CSV > Enable Auto Convert`
- `Tools > CSV > Disable Auto Convert`

---

## 에디터 명령어

### Tools > CSV

| 명령어 | 설명 |
|--------|------|
| `Force Reconvert All` | 모든 CSV 강제 재변환 |
| `Enable Auto Convert` | 자동 변환 활성화 |
| `Disable Auto Convert` | 자동 변환 비활성화 |

### Tools > Addressables

| 명령어 | 설명 |
|--------|------|
| `Build Addressables` | Addressables 빌드 |

---

## UI 컨벤션

### Container 왼쪽 정렬

```
Anchor: Min (0, 0.5), Max (0, 0.5)
Pivot: (0, 0.5)
ChildAlignment: Lower Left (3)
```

### 캐시 무효화 패턴

```csharp
private bool _cacheDirty = true;
private Stats _cachedStats;

public Stats Stats
{
    get
    {
        if (_cacheDirty)
        {
            _cachedStats = CalculateStats();
            _cacheDirty = false;
        }
        return _cachedStats;
    }
}

public void InvalidateCache() => _cacheDirty = true;
```

---

## Setup 문서 관리

| 상황 | 동작 |
|------|------|
| 새 내용 추가 | 기존 내용 → `Setup_archive.md`, 새 내용 → `Setup.md` |
| archive 500줄 초과 | `Setup_archive_1.md` 생성 |

---

## 외부 패키지

| 패키지 | 용도 |
|--------|------|
| `Unity Addressables` | 에셋 로드 |
| `GameDevToolkit` | 재사용 도구 모음 (DI 내장) |
| `GameDevToolkit` | 재사용 도구 모음 |