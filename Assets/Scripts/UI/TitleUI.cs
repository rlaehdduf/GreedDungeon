using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GreedDungeon.UI
{
    public class TitleUI : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private float _walkDuration = 3f;
        [SerializeField] private float _zoomSpeed = 0.3f;
        [SerializeField] private float _bobSpeed = 8f;
        [SerializeField] private float _bobAmount = 15f;
        [SerializeField] private float _shakeAmount = 3f;

        [Header("Blinking Text")]
        [SerializeField] private Text _blinkText;
        [SerializeField] private float _blinkSpeed = 2f;
        [Range(0f, 1f)] [SerializeField] private float _blinkMinAlpha = 0.2f;
        [Range(0f, 1f)] [SerializeField] private float _blinkMaxAlpha = 1f;

        private bool _isTransitioning;
        private Vector3 _originalScale;

        private void Start()
        {
            if (_background != null)
                _originalScale = _background.localScale;
        }

        private void Update()
        {
            if (_blinkText != null)
            {
                float t = (Mathf.Sin(Time.time * _blinkSpeed) + 1f) / 2f;
                float alpha = Mathf.Lerp(_blinkMinAlpha, _blinkMaxAlpha, t);
                var color = _blinkText.color;
                color.a = alpha;
                _blinkText.color = color;
            }

            if (!_isTransitioning && IsAnyInputPressed())
            {
                StartCoroutine(WalkTransition());
            }
        }

        private bool IsAnyInputPressed()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            return false;
        }

        private IEnumerator WalkTransition()
        {
            _isTransitioning = true;

            float elapsed = 0f;
            Vector3 currentScale = _originalScale;
            Vector3 originalPos = _background.anchoredPosition;

            while (elapsed < _walkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _walkDuration;

                currentScale = _originalScale * (1f + t * _zoomSpeed);
                _background.localScale = currentScale;

                float bob = Mathf.Sin(elapsed * _bobSpeed) * _bobAmount;
                float microBob = Mathf.Sin(elapsed * _bobSpeed * 3f) * _shakeAmount;
                
                _background.anchoredPosition = new Vector2(
                    originalPos.x,
                    originalPos.y + bob + microBob
                );

                yield return null;
            }

            SceneManager.LoadScene("Battle");
        }
    }
}