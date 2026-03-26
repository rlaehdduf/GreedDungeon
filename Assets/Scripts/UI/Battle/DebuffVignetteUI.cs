using System.Collections;
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
        [SerializeField] private Color _damageColor = new Color(1f, 0.2f, 0.2f, 0.6f);

        [Header("Damage Flash")]
        [SerializeField] private float _damageFlashDuration = 0.15f;

        private Player _player;
        private Coroutine _currentFade;
        private Color _savedDebuffColor = Color.clear;

        private void Awake()
        {
            if (_vignetteImage != null)
            {
                _vignetteImage.color = Color.clear;
            }
        }

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

            if (_vignetteImage != null)
            {
                _vignetteImage.color = Color.clear;
            }
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
            _savedDebuffColor = GetCurrentDebuffColor();
            if (_currentFade == null)
            {
                UpdateVignette();
            }
        }

        public void ShowDamageFlash()
        {
            if (_vignetteImage == null) return;
            
            StartCoroutine(DamageFlashCoroutine());
        }

        private IEnumerator DamageFlashCoroutine()
        {
            Color startColor = _vignetteImage.color;
            
            if (_currentFade != null)
                StopCoroutine(_currentFade);

            _vignetteImage.color = _damageColor;
            
            yield return new WaitForSeconds(_damageFlashDuration);
            
            Color targetColor = _savedDebuffColor;
            float elapsed = 0f;
            float fadeTime = 0.2f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _vignetteImage.color = Color.Lerp(_damageColor, targetColor, elapsed / fadeTime);
                yield return null;
            }

            _vignetteImage.color = targetColor;
        }

        private Color GetCurrentDebuffColor()
        {
            if (_player == null || _player.StatusEffects.Count == 0)
                return Color.clear;

            var lastDebuff = _player.StatusEffects[_player.StatusEffects.Count - 1];
            return GetDebuffColor(lastDebuff.Data?.ID ?? 0);
        }

        private void UpdateVignette()
        {
            if (_vignetteImage == null) return;

            Color targetColor = GetCurrentDebuffColor();
            _currentFade = StartCoroutine(FadeToColor(targetColor));
        }

        private IEnumerator FadeToColor(Color targetColor)
        {
            if (_vignetteImage == null) yield break;

            Color startColor = _vignetteImage.color;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _vignetteImage.color = Color.Lerp(startColor, targetColor, elapsed / _fadeDuration);
                yield return null;
            }

            _vignetteImage.color = targetColor;
            _currentFade = null;
        }

        private Color GetDebuffColor(int id)
        {
            return id switch
            {
                1 => _burnColor,
                2 => _poisonColor,
                3 => _stunColor,
                _ => Color.clear
            };
        }
    }
}