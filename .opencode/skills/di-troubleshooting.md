# DI (Dependency Injection) 트러블슈팅

## 프로젝트 DI 시스템

**사용 중인 DI:** Pathfinder.Core.DI

### 핵심 컴포넌트

| 컴포넌트 | 역할 |
|---------|------|
| `RootContext` | DI 컨테이너 초기화, Installer 실행 |
| `Installer` | 서비스 등록 (인터페이스 → 구현체 매핑) |
| `[Inject]` | 의존성 주입 표시 (생성된 객체에서만 작동) |
| `DIContainer` | 서비스 저장소, Resolve()로 가져오기 |

### 서비스 등록 방식 (GameInstaller.cs)

```csharp
container.Register<IGameManager, GameManager>(lifetime: ServiceLifetime.Singleton);
```

---

## 문제: NullReferenceException on [Inject] field

### 증상
```
NullReferenceException: Object reference not set to an instance of an object
```
`[Inject]` 필드가 null인 상태

### 원인 분석

`[Inject]` 속성은 **DI 컨테이너가 생성한 객체**에서만 작동합니다.

| 객체 타입 | 생성 주체 | [Inject] 작동 여부 |
|----------|----------|-------------------|
| 일반 클래스 | DI 컨테이너 | ✅ 자동 주입 |
| MonoBehaviour | Unity (AddComponent) | ❌ 수동 처리 필요 |
| ScriptableObject | Unity (CreateInstance) | ❌ 수동 처리 필요 |

### 해결 방법

#### 방법 A: 서비스 로케이터 패턴 (이 프로젝트에서 사용)

**Services.cs** 정적 클래스 사용:

```csharp
public static class Services
{
    private static DIContainer _container;
    
    public static void Initialize(DIContainer container) => _container = container;
    public static T Get<T>() where T : class => _container.Resolve<T>();
}
```

MonoBehaviour에서 사용:
```csharp
public class MyComponent : MonoBehaviour
{
    private IGameDataManager _gameDataManager;

    private void Awake()
    {
        _gameDataManager = Services.Get<IGameDataManager>();
    }
}
```

GameInstaller에서 초기화:
```csharp
public override void Install(DIContainer container)
{
    Services.Initialize(container);
    // ... 서비스 등록
}
```

#### 방법 B: RootContext에서 직접 Resolve

```csharp
public class MyComponent : MonoBehaviour
{
    private IGameDataManager _gameDataManager;

    private void Awake()
    {
        var rootContext = GetComponentInParent<RootContext>();
        if (rootContext != null)
        {
            _gameDataManager = rootContext.Container.Resolve<IGameDataManager>();
        }
    }
}
```

#### 방법 C: MonoBehaviourInjection (Pathfinder 지원 시)

Pathfinder가 MonoBehaviour 자동 주입을 지원하는지 확인 필요.

---

## 체크리스트

DI 문제 발생 시:

- [ ] RootContext가 씬에 있는가?
- [ ] GameInstaller가 RootContext에 등록되었는가?
- [ ] 필요한 서비스가 Installer에 등록되었는가?
- [ ] MonoBehaviour인가? → 수동 Resolve 필요
- [ ] 초기화 순서가 올바른가? (Awake vs Start)

---

## 이 프로젝트의 등록된 서비스

```csharp
// GameInstaller.cs
container.Register<IGameManager, GameManager>();
container.Register<ISceneLoader, SceneLoader>();
container.Register<IAssetLoader, AddressablesLoader>();
container.Register<IGameDataManager, GameDataManager>();
```

### Resolve 방법 (MonoBehaviour)

```csharp
// Services.Get<T>() 사용
var gameDataManager = Services.Get<IGameDataManager>();
var assetLoader = Services.Get<IAssetLoader>();
```

| 서비스 | Resolve 호출 |
|-------|-------------|
| IGameManager | `Services.Get<IGameManager>()` |
| ISceneLoader | `Services.Get<ISceneLoader>()` |
| IAssetLoader | `Services.Get<IAssetLoader>()` |
| IGameDataManager | `Services.Get<IGameDataManager>()` |

---

## 관련 파일

- `Assets/Scripts/Core/Services.cs` - 서비스 로케이터 (정적 접근)
- `Assets/Scripts/Core/GameInstaller.cs` - 서비스 등록
- `Assets/Scripts/Core/` - IAssetLoader, AddressablesLoader, GameDataManager 등
- `Setup.md` - RootContext 설정 가이드