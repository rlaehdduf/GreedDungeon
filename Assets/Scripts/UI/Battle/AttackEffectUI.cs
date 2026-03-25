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

        private void Awake()
        {
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

        public async void ShowEffect(SkillType skillType)
        {
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
            catch
            {
            }

            if (sprite == null || _effectImage == null) return;

            _effectImage.sprite = sprite;
            _effectImage.enabled = true;
            _effectImage.color = Color.white;

            Invoke(nameof(FadeOut), _displayDuration);
        }

        private void FadeOut()
        {
            if (_effectImage == null) return;
            StartCoroutine(FadeOutCoroutine());
        }

        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            float elapsed = 0f;
            Color startColor = _effectImage.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _effectImage.color = UnityEngine.Color.Lerp(startColor, endColor, elapsed / _fadeDuration);
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