# GreedDungeon 개발 진행상황

## 목표
던전 크롤러 턴제 RPG 프로토타입 개발 (Unity 2D)

## 개발 규칙
- **항상 물어보고 진행하기** - 작업 전 사용자 승인 필수
- **Unity 수동 설정 사항은 Setup.md에 기록하고 Git 커밋**
- **Pathfinder Core DI 시스템 사용** - `[Inject]` 속성으로 의존성 주입
- **SOLID 준수** - 클래스 300행/메서드 10개 제한
- **CSV 데이터 워크플로우**: GameData.xlsm → 자동 CSV 저장 → CSVConverter → ScriptableObject
- **Addressables 방식 B 사용**: 커스텀 주소 (예: Monsters/Slime)

## 발견 사항 (Discoveries)
1. **Unity CLI 연결 문제** - HTTP server는 실행되지만 명령이 타임아웃됨. 파일 조작으로 우회.
2. **CSV 파싱 UTF-8 BOM 문제 해결**: `File.ReadAllBytes()`로 직접 읽고 UTF-8 BOM 감지 후 수동 디코딩
3. **생성자 순서 버그**: 추상 클래스에서 `abstract property`는 자식 생성자보다 늦게 초기화됨 → `InitializeStats()` 패턴으로 해결
4. **Resources 폴더 보안 이슈**: Resources 폴더의 모든 파일은 빌드에 무조건 포함 → EditorData 폴더로 이동하여 해결
5. **Excel MCP Server 설치 완료**: `uv` 패키지 매니저 설치 후 opencode.json에 MCP 설정 추가
6. **VBA 매크로로 자동 CSV 저장 구현**: 엑셀 저장 시 자동으로 csv 폴더에 CSV 파일 생성
7. **Windows `nul` 파일 문제**: Windows 예약어라 git에서 오류 발생. 제거 필요.

## 완료된 작업 (Accomplished)

### Phase 1: 전투 시스템 기초 ✅
- 턴제 전투 시스템 구현
- 기본 스탯 시스템 (HP, MP, 공격력, 방어력, 속도, 크리티컬)
- 전투 로그 시스템

### Phase 2: 속성 & 상태이상 ✅
- 5가지 속성 시스템 (무, 불, 물, 풀, 전기)
- 상태이상 시스템 (화상, 독, 스턴)
- 속성 상성 보너스

### Phase 4: 인벤토리/버프/아이템 사용 ✅
- 인벤토리 시스템
- 버프/디버프 시스템
- 소모품 아이템 사용

### 데이터 워크플로우 구축 ✅
- CSV 파일 구조 정리: `Assets/EditorData/Data/csv/` 폴더로 이동
- CSVConverter 경로 수정 완료
- ScriptableObject에 PrefabAddress/IconAddress 필드 추가 완료
- CSVConverter 새 컬럼 처리 추가 완료
- Excel MCP Server 설치 및 연결 완료
- VBA 매크로 코드 적용 완료 (GameData.xlsm)
- GameData.xlsx 삭제, GameData.xlsm 유지

## 진행 중인 작업 (In Progress)
- Unity에서 Addressables 주소 설정 (프리팹 43개)

## 대기 중인 작업 (Pending)
- Unity CSVConverter 실행 테스트
- Phase 3: UI 시스템
- Phase 5: 스킬 시스템 구현

## 폴더 구조

```
Assets/
├── EditorData/Data/
│   ├── GameData.xlsm          ← 엑셀 원본 (VBA 매크로 포함)
│   └── csv/                    ← CSV 파일들 (VBA로 자동 생성)
│       ├── MonsterData.csv     (15개 컬럼: ID ~ PrefabAddress)
│       ├── SkillData.csv       (16개 컬럼: ID ~ IconAddress)
│       ├── EquipmentData.csv   (13개 컬럼: ID ~ IconAddress)
│       ├── ConsumableData.csv  (11개 컬럼: ID ~ IconAddress)
│       ├── RarityData.csv
│       └── StatusEffect.csv
│
├── Prefabs/
│   ├── Monster/                ← 몬스터 프리팹 (5개)
│   │   ├── Slime(Leaf).prefab
│   │   ├── Rat(None).prefab
│   │   ├── Spider(Leaf).prefab
│   │   ├── Skull(Fire).prefab
│   │   └── Cerberus(Fire).prefab
│   ├── SkillIcon/              ← 스킬 아이콘 (15개)
│   ├── WeaponIcon/             ← 무기 아이콘 (5개)
│   ├── ArmorIcon/              ← 갑옷 아이콘 (5개)
│   ├── Accessory/              ← 악세서리 아이콘 (5개)
│   └── ItemIcon/               ← 아이템 아이콘 (8개)
│
├── Scripts/
│   ├── ScriptableObjects/
│   │   ├── MonsterDataSO.cs      ← PrefabAddress 필드 추가됨
│   │   ├── SkillDataSO.cs        ← IconAddress, ValueFloat, HitCount, Cooldown 필드 추가됨
│   │   ├── EquipmentDataSO.cs    ← IconAddress 필드 추가됨
│   │   └── ConsumableDataSO.cs   ← IconAddress 필드 추가됨
│   └── Editor/
│       └── CSVConverter.cs       ← 경로 수정, 새 컬럼 처리 추가됨
│
└── ScriptableObjects/Data/       ← 변환된 데이터
    ├── Monsters/ (5개)
    ├── Skills/ (15개)
    ├── Equipments/ (15개)
    ├── Consumables/ (10개)
    ├── Rarities/ (5개)
    └── StatusEffects/ (3개)
```

## VBA 매크로 코드 (GameData.xlsm에 적용됨)

```vba
Private Sub Workbook_BeforeSave(ByVal SaveAsUI As Boolean, Cancel As Boolean)
    Dim ws As Worksheet
    Dim csvPath As String
    Dim csvFile As String
    
    csvPath = ThisWorkbook.Path & "\csv\"
    
    For Each ws In ThisWorkbook.Worksheets
        csvFile = csvPath & ws.Name & ".csv"
        ws.Copy
        ActiveWorkbook.SaveAs Filename:=csvFile, FileFormat:=xlCSVUTF8, CreateBackup:=False
        ActiveWorkbook.Close SaveChanges:=False
    Next ws
    
    MsgBox "CSV 파일 저장 완료!", vbInformation
End Sub
```

## 다음 단계

1. Unity에서 CSVConverter 실행하여 ScriptableObject 업데이트
2. Unity CLI로 Addressables 주소 일괄 설정
3. 런타임에 Addressables로 프리팹 로드 테스트
4. Phase 3 UI 시스템 구현

---
최종 업데이트: 2026-03-20