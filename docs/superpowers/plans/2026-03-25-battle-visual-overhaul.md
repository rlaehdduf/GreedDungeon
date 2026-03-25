# 전투 시스템 비주얼 개편 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 전투 시스템에 시각적 효과와 애니메이션을 추가하여 전투 경험 개선

**Architecture:** 코루틴 기반 딜레이 시스템, 오브젝트 풀링, UI 이펙트 컴포넌트 분리

**Tech Stack:** Unity 2D, C#, Coroutines, Object Pooling, Addressables

---

## 파일 구조

### 신규 파일
- `Assets/Scripts/UI/Battle/DamageTextUI.cs` - 데미지 텍스트 풀 및 애니메이션
- `Assets/Scripts/UI/Battle/AttackEffectUI.cs` - 공격 이펙트 표시
- `Assets/Scripts/UI/Battle/DebuffVignetteUI.cs` - 화면 테두리 그라데이션
- `Assets/Prefabs/UI/DamageText.prefab` - 데미지 텍스트 프리팹
- `Assets/Prefabs/UI/AttackEffect.prefab` - 공격 이펙트 프리팹

### 수정 파일
- `Assets/Scripts/Combat/BattleManager.cs` - 딜레이 시스템 추가
- `Assets/Scripts/Combat/BattleController.cs` - 딜레이 적용
- `Assets/Scripts/UI/Battle/MonsterDisplay.cs` - 공격 모션, 디버프 색상
- `Assets/Scripts/UI/Battle/BattleUI.cs` - 이펙트/데미지 텍스트 연결
- `Assets/Scripts/UI/Battle/PlayerStatusUI.cs` - 디버프 이펙트 연결

---

## Task 1: 턴 딜레이 시스템

**Files:**
- Modify: `Assets/Scripts/Combat/BattleManager.cs`

- [ ] **Step 1: BattleManager에 딜레이 설정 추가**

```csharp
[Header("Delay Settings")]
[SerializeField] private float _attackStartDelay = 0.3f;
[SerializeField] private float _effectDisplayDelay = 0.5f;
[SerializeField] private float _afterDamageDelay = 0.5f;
[SerializeField] private float _turnTransitionDelay = 0.3f;
```

- [ ] **Step 2: 딜레이용 코루틴 메서드 추가**

```csharp
public Coroutine WaitForSeconds(float seconds)
{
    return _monoBehaviour.StartCoroutine(WaitCoroutine(seconds));
}

private IEnumerator WaitCoroutine(float seconds)
{
    yield return new WaitForSeconds(seconds);
}
```

- [ ] **Step 3: BattleController에 딜레이 적용**

`HandleAttack`, `HandleSkillUsed` 메서드에 딜레이 추가

- [ ] **Step 4: 커밋**

```bash
git add Assets/Scripts/Combat/BattleManager.cs Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: 전투 턴 딜레이 시스템 추가"
```

---

## Task 2: 적 공격 모션

**Files:**
- Modify: `Assets/Scripts/UI/Battle/MonsterDisplay.cs`

- [ ] **Step 1: 공격 모션 코루틴 추가**

```csharp
[Header("Animation")]
[SerializeField] private float _attackScale = 1.4f;
[SerializeField] private float _attackDuration = 0.4f;

public Coroutine PlayAttackAnimation()
{
    return StartCoroutine(AttackAnimationCoroutine());
}

private IEnumerator AttackAnimationCoroutine()
{
    Vector3 originalScale = transform.localScale;
    Vector3 targetScale = originalScale * _attackScale;
    
    float elapsed = 0f;
    while (elapsed < _attackDuration / 2)
    {
        transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (_attackDuration / 2));
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    elapsed = 0f;
    while (elapsed < _attackDuration / 2)
    {
        transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (_attackDuration / 2));
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    transform.localScale = originalScale;
}
```

- [ ] **Step 2: BattleController에서 모션 호출**

`ExecuteMonsterTurn` 메서드에 모션 추가

- [ ] **Step 3: 커밋**

```bash
git add Assets/Scripts/UI/Battle/MonsterDisplay.cs Assets/Scripts/Combat/BattleController.cs
git commit -m "feat: 적 공격 모션 (스케일 확대) 추가"
```

---

## Task 3: 데미지 텍스트 표시

**Files:**
- Create: `Assets/Scripts/UI/Battle/DamageTextUI.cs`
- Create: `Assets/Prefabs/UI/DamageText.prefab`

- [ ] **Step 1: DamageTextUI 스크립트 작성**

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace GreedDungeon.UI.Battle
{
    public class DamageTextUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject _damageTextPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private int _poolSize = 10;
        
        [Header("Animation")]
        [SerializeField] private float _moveUpDistance = 50f;
        [SerializeField] private float _animationDuration = 0.8f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private readonly Queue<GameObject> _pool = new();
        
        private void Awake()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var go = Instantiate(_damageTextPrefab, _container);
                go.SetActive(false);
                _pool.Enqueue(go);
            }
        }
        
        public void ShowDamage(int damage, Vector2 position, bool isCritical = false, bool isHeal = false)
        {
            var text = GetText();
            text.transform.localPosition = position;
            text.SetActive(true);
            
            var textComponent = text.GetComponent<Text>();
            textComponent.text = isHeal ? $"+{damage}" : $"-{damage}";
            textComponent.color = GetColor(isCritical, isHeal);
            
            text.transform.DOLocalMoveY(position.y + _moveUpDistance, _animationDuration)
                .SetEase(Ease.OutQuad);
            
            textComponent.DOFade(0f, _fadeDuration)
                .SetDelay(_animationDuration - _fadeDuration)
                .OnComplete(() => ReturnText(text));
        }
        
        private GameObject GetText()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();
            return Instantiate(_damageTextPrefab, _container);
        }
        
        private void ReturnText(GameObject text)
        {
            text.SetActive(false);
            _pool.Enqueue(text);
        }
        
        private Color GetColor(bool isCritical, bool isHeal)
        {
            if (isHeal) return Color.green;
            if (isCritical) return Color.yellow;
            return Color.white;
        }
    }
}
```

- [ ] **Step 2: 데미지 텍스트 프리팹 생성 (Unity 수동)**
- Text 컴포넌트가 있는 GameObject 생성
- 폰트 크기 36, Outline 추가

- [ ] **Step 3: BattleUI에 DamageTextUI 연결**

- [ ] **Step 4: BattleManager에서 데미지 텍스트 호출**

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/UI/Battle/DamageTextUI.cs Assets/Scripts/UI/Battle/BattleUI.cs Assets/Scripts/Combat/BattleManager.cs
git commit -m "feat: 데미지 텍스트 표시 시스템 추가"
```

---

## Task 4: 공격 이펙트

**Files:**
- Create: `Assets/Scripts/UI/Battle/AttackEffectUI.cs`
- Create: `Assets/Prefabs/UI/AttackEffect.prefab`

- [ ] **Step 1: AttackEffectUI 스크립트 작성**

```csharp
using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class AttackEffectUI : MonoBehaviour
    {
        [Header("Effect Images")]
        [SerializeField] private Image _effectImage;
        [SerializeField] private float _displayDuration = 0.5f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private IAssetLoader _assetLoader;
        
        private void Start()
        {
            if (Services.IsInitialized)
                _assetLoader = Services.Get<IAssetLoader>();
            
            _effectImage.enabled = false;
        }
        
        public async void ShowEffect(EffectType effectType)
        {
            string address = GetEffectAddress(effectType);
            if (string.IsNullOrEmpty(address)) return;
            
            if (_assetLoader == null && Services.IsInitialized)
                _assetLoader = Services.Get<IAssetLoader>();
            
            var sprite = await _assetLoader.LoadAssetAsync<Sprite>(address);
            if (sprite == null) return;
            
            _effectImage.sprite = sprite;
            _effectImage.enabled = true;
            _effectImage.color = Color.white;
            
            Invoke(nameof(FadeOut), _displayDuration);
        }
        
        private void FadeOut()
        {
            _effectImage.CrossFadeAlpha(0f, _fadeDuration, false);
            Invoke(nameof(Hide), _fadeDuration);
        }
        
        private void Hide()
        {
            _effectImage.enabled = false;
        }
        
        private string GetEffectAddress(EffectType type)
        {
            return type switch
            {
                EffectType.Melee => "Effects/Melee",
                EffectType.Magic => "Effects/Magic",
                _ => "Effects/Neutral"
            };
        }
    }
}
```

- [ ] **Step 2: 이펙트 스프라이트 리소스 준비 (Unity 수동)**
- `Assets/Sprites/Effects/Neutral.png`
- `Assets/Sprites/Effects/Melee.png`
- `Assets/Sprites/Effects/Magic.png`
- Addressables 등록

- [ ] **Step 3: BattleUI에 AttackEffectUI 연결**

- [ ] **Step 4: BattleManager에서 이펙트 호출**

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/UI/Battle/AttackEffectUI.cs Assets/Scripts/UI/Battle/BattleUI.cs
git commit -m "feat: 공격 이펙트 시스템 추가"
```

---

## Task 5: 디버프 시각 효과 (화면 테두리 그라데이션)

**Files:**
- Create: `Assets/Scripts/UI/Battle/DebuffVignetteUI.cs`

- [ ] **Step 1: DebuffVignetteUI 스크립트 작성**

```csharp
using System.Collections.Generic;
using GreedDungeon.Character;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class DebuffVignetteUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image _vignetteImage;
        [SerializeField] private float _fadeDuration = 0.5f;
        
        [Header("Colors")]
        [SerializeField] private Color _burnColor = new Color(1f, 0.27f, 0.27f, 0.5f);
        [SerializeField] private Color _poisonColor = new Color(0.27f, 1f, 0.27f, 0.5f);
        [SerializeField] private Color _stunColor = new Color(1f, 1f, 0.27f, 0.5f);
        
        private Player _player;
        private readonly Dictionary<int, Color> _debuffColors = new();
        
        public void Setup(Player player)
        {
            if (_player != null)
            {
                _player.OnStatusEffectApplied -= OnDebuffChanged;
                _player.OnStatusEffectEnded -= OnDebuffChanged;
            }
            
            _player = player;
            
            if (_player != null)
            {
                _player.OnStatusEffectApplied += OnDebuffChanged;
                _player.OnStatusEffectEnded += OnDebuffChanged;
            }
            
            _vignetteImage.color = Color.clear;
        }
        
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnStatusEffectApplied -= OnDebuffChanged;
                _player.OnStatusEffectEnded -= OnDebuffChanged;
            }
        }
        
        private void OnDebuffChanged(IBattleEntity entity, ActiveStatusEffect effect)
        {
            UpdateVignette();
        }
        
        private void UpdateVignette()
        {
            if (_player == null || _player.StatusEffects.Count == 0)
            {
                FadeOut();
                return;
            }
            
            var lastDebuff = _player.StatusEffects[_player.StatusEffects.Count - 1];
            Color targetColor = GetDebuffColor(lastDebuff.Data?.StatusEffectID ?? 0);
            
            _vignetteImage.CrossFadeColor(targetColor, _fadeDuration, false, true);
        }
        
        private void FadeOut()
        {
            _vignetteImage.CrossFadeColor(Color.clear, _fadeDuration, false, true);
        }
        
        private Color GetDebuffColor(int statusEffectID)
        {
            return statusEffectID switch
            {
                1 => _burnColor,    // Burn
                2 => _poisonColor,  // Poison
                3 => _stunColor,    // Stun
                _ => Color.clear
            };
        }
    }
}
```

- [ ] **Step 2: 그라데이션 비네트 스프라이트 생성 (Unity 수동)**
- 가장자리는 불투명, 중앙은 투명한 원형 그라데이션
- `Assets/Sprites/UI/Vignette.png`

- [ ] **Step 3: Canvas에 DebuffVignette Image 추가 (Unity 수동)**
- 화면 전체 크기
- DebuffVignetteUI 컴포넌트 추가

- [ ] **Step 4: BattleUI에 DebuffVignetteUI 연결**

- [ ] **Step 5: MonsterDisplay에 디버프 색상 오버레이 추가**

```csharp
public void UpdateDebuffColor(int statusEffectID)
{
    if (_spriteRenderer == null) return;
    
    Color debuffColor = statusEffectID switch
    {
        1 => new Color(1f, 0.5f, 0.5f),  // Burn
        2 => new Color(0.5f, 1f, 0.5f),  // Poison
        3 => new Color(1f, 1f, 0.5f),    // Stun
        _ => Color.white
    };
    
    _spriteRenderer.color = debuffColor;
}
```

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/UI/Battle/DebuffVignetteUI.cs Assets/Scripts/UI/Battle/MonsterDisplay.cs Assets/Scripts/UI/Battle/BattleUI.cs
git commit -m "feat: 디버프 시각 효과 (화면 테두리 그라데이션) 추가"
```

---

## Task 6: 통합 테스트 및 문서 업데이트

**Files:**
- Modify: `docs/setup/Setup.md`
- Modify: `CONTEXT.md`

- [ ] **Step 1: Battle 씬 UI 설정 (Unity 수동)**
- DamageTextContainer 추가
- EffectContainer 추가
- DebuffVignette 추가

- [ ] **Step 2: Setup.md에 설정 가이드 추가**

- [ ] **Step 3: CONTEXT.md 업데이트**

- [ ] **Step 4: 최종 커밋**

```bash
git add docs/setup/Setup.md CONTEXT.md
git commit -m "docs: 전투 비주얼 시스템 문서 업데이트"
```

---

## Unity 수동 작업 목록

1. **이펙트 스프라이트 준비**
   - `Assets/Sprites/Effects/Neutral.png`
   - `Assets/Sprites/Effects/Melee.png`
   - `Assets/Sprites/Effects/Magic.png`
   - Addressables 주소: `Effects/Neutral`, `Effects/Melee`, `Effects/Magic`

2. **프리팹 생성**
   - `Assets/Prefabs/UI/DamageText.prefab` - Text 컴포넌트
   - `Assets/Prefabs/UI/AttackEffect.prefab` - Image 컴포넌트

3. **비네트 스프라이트**
   - `Assets/Sprites/UI/Vignette.png` - 원형 그라데이션

4. **Battle 씬 설정**
   - DamageTextContainer 추가
   - EffectContainer 추가
   - DebuffVignette (화면 전체 Image) 추가
   - BattleUI 컴포넌트에 각 요소 연결