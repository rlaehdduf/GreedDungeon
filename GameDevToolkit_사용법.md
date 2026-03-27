# GameDevToolkit 사용법

Unity 게임 개발 범용 패키지

---

## 설치

### 1. manifest.json 수정

`프로젝트/Packages/manifest.json`에 추가:

```json
{
  "dependencies": {
    "com.gamedev.toolkit": "file:C:/Users/admin/GameDevToolkit"
  }
}
```

### 2. 필수 패키지 설치

Unity Package Manager에서 설치:
- Addressables

---

## 초기 설정

### 1. 문서 복사

```
Unity > Tools > GameDevToolkit > Copy All Docs to Project
```

### 2. RootContext 설정

1. Hierarchy에서 빈 GameObject 생성 (이름: RootContext)
2. `RootContext` 컴포넌트 추가
3. Installer 스크립트 생성 후 등록

### 3. Installer 작성

```csharp
using GameDevToolkit.Core;
using GameDevToolkit.Core.DI;

public class GameInstaller : Installer
{
    public override void Install(DIContainer container)
    {
        Services.Initialize(container);
        container.Register<IAssetLoader, AddressablesLoader>(lifetime: ServiceLifetime.Singleton);
    }
}
```

---

## CSV 워크플로우

### 폴더 구조

```
Assets/
└── EditorData/
    └── Data/
        ├── GameData.xlsm    # Excel 데이터
        └── csv/             # CSV 파일
            ├── MonsterData.csv
            ├── SkillData.csv
            └── ...
```

### 자동 변환

```
Tools > CSV > Enable Auto Convert   # 켜기
Tools > CSV > Disable Auto Convert  # 끄기
```

### 검증

```
Tools > CSV > Validate All Data     # 데이터 검증
```

---

## 주요 기능

### Services (DI)

```csharp
using GameDevToolkit.Core;

// 서비스 가져오기
var loader = Services.Get<IAssetLoader>();
```

### AssetLoader

```csharp
using GameDevToolkit.Core;

private IAssetLoader _loader;

private async void Start()
{
    _loader = Services.Get<IAssetLoader>();
    
    // 단일 에셋
    var sprite = await _loader.LoadAssetAsync<Sprite>("UI/Icon");
    
    // 라벨로 여러 에셋
    var monsters = await _loader.LoadAllAssetsByLabelAsync<MonsterData>("MonsterData");
    
    // 해제
    _loader.ReleaseAsset(sprite);
}
```

### UIView

```csharp
using GameDevToolkit.UI;

public class MyPanel : UIView
{
    public override void Show()
    {
        // 초기화 로직
        base.Show();
    }
}

// 사용
panel.Show();
panel.Hide();
panel.Toggle();
```

---

## 에디터 명령어

### Tools > CSV

| 명령어 | 설명 |
|--------|------|
| Validate All Data | CSV 데이터 검증 |
| Validate & Show Statistics | 검증 + 통계 |
| Force Reconvert All | 강제 재변환 |
| Enable Auto Convert | 자동 변환 켜기 |
| Disable Auto Convert | 자동 변환 끄기 |

### Tools > GameDevToolkit

| 명령어 | 설명 |
|--------|------|
| Copy All Docs to Project | 문서 복사 |
| Show Changelog | 변경 이력 |

---

## 패키지 위치

```
C:/Users/admin/GameDevToolkit
```

---

## 버전

현재: v1.5.0

---

## 파일 구조

```
GameDevToolkit/
├── Runtime/
│   ├── Core/
│   │   ├── DI/              # DI 컨테이너
│   │   ├── Services.cs
│   │   ├── IAssetLoader.cs
│   │   └── AddressablesLoader.cs
│   └── UI/
│       └── UIView.cs
├── Editor/
│   ├── CSVConverter.cs
│   ├── AutoCSVConverter.cs
│   ├── AddressablesSetter.cs
│   ├── DataValidator.cs
│   └── SetupHelper.cs
└── Samples~/
    └── BasicUsage/          # 예시 코드
```

---

## 문의

로컬 전용 패키지