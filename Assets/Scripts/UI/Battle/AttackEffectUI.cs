using System.Collections;
using System.Threading.Tasks;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class AttackEffectUI : MonoBehaviour
    {
        [Header("Effect Settings")]
        [SerializeField] private Image _effectImage;
        [SerializeField] private float _displayDuration = 0.4f;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private float _effectInterval = 0.12f;
        [SerializeField] private float _randomOffset = 50f;

        private IAssetLoader _assetLoader;
        private Coroutine _currentCoroutine;

        private void Awake()
        {
            if (_effectImage == null)
                _effectImage = GetComponent<Image>();
            
            if (_effectImage != null)
            {
                _effectImage.enabled = false;
            }
        }

        private void Start()
        {
            if (Services.IsInitialized)
                _assetLoader = Services.Get<IAssetLoader>();
        }

        public async void ShowEffect(SkillType skillType, int hitCount = 1)
        {
            if (_effectImage == null)
                _effectImage = GetComponent<Image>();

            if (_effectImage == null) return;

            string address = GetEffectAddress(skillType);
            if (string.IsNullOrEmpty(address)) return;

            if (_assetLoader == null && Services.IsInitialized)
                _assetLoader = Services.Get<IAssetLoader>();

            if (_assetLoader == null) return;

            Sprite sprite = null;
            try
            {
                sprite = await _assetLoader.LoadAssetAsync<Sprite>(address);
            }
            catch { }

            if (sprite == null) return;

            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);
            
            _currentCoroutine = StartCoroutine(ShowMultipleEffects(sprite, hitCount));
        }

        private IEnumerator ShowMultipleEffects(Sprite sprite, int hitCount)
        {
            Vector2 originalPosition = _effectImage.rectTransform.anchoredPosition;
            int actualHits = Mathf.Max(1, hitCount);

            for (int i = 0; i < actualHits; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Random.Range(-_randomOffset, _randomOffset),
                    Random.Range(-_randomOffset, _randomOffset)
                );
                
                _effectImage.rectTransform.anchoredPosition = originalPosition + randomOffset;
                _effectImage.sprite = sprite;
                _effectImage.enabled = true;
                _effectImage.color = Color.white;

                yield return new WaitForSeconds(_displayDuration);
                yield return StartCoroutine(FadeOutEffect());
                
                if (i < actualHits - 1)
                {
                    yield return new WaitForSeconds(_effectInterval);
                }
            }

            _effectImage.rectTransform.anchoredPosition = originalPosition;
            _currentCoroutine = null;
        }

        private IEnumerator FadeOutEffect()
        {
            if (_effectImage == null) yield break;
            
            float elapsed = 0f;
            Color startColor = _effectImage.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _effectImage.color = Color.Lerp(startColor, endColor, elapsed / _fadeDuration);
                yield return null;
            }

            _effectImage.enabled = false;
        }

        private string GetEffectAddress(SkillType type)
        {
            return type switch
            {
                SkillType.Melee => "Effects/Melee",
                SkillType.Magic => "Effects/Magic",
                _ => "Effects/Neutral"
            };
        }
    }
}