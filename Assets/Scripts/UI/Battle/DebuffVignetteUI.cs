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
        private bool _isFading;

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
            UpdateVignette();
        }

        private void UpdateVignette()
        {
            if (_vignetteImage == null) return;

            if (_player == null || _player.StatusEffects.Count == 0)
            {
                FadeOut();
                return;
            }

            var lastDebuff = _player.StatusEffects[_player.StatusEffects.Count - 1];
            Color targetColor = GetDebuffColor(lastDebuff.Data?.ID ?? 0);

            StartCoroutine(FadeToColor(targetColor));
        }

        private void FadeOut()
        {
            StartCoroutine(FadeToColor(Color.clear));
        }

        private System.Collections.IEnumerator FadeToColor(Color targetColor)
        {
            if (_vignetteImage == null || _isFading) yield break;

            _isFading = true;
            Color startColor = _vignetteImage.color;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _vignetteImage.color = Color.Lerp(startColor, targetColor, elapsed / _fadeDuration);
                yield return null;
            }

            _vignetteImage.color = targetColor;
            _isFading = false;
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