using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Font")]
        [SerializeField] private int _fontSize = 36;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _criticalColor = Color.yellow;
        [SerializeField] private Color _healColor = Color.green;
        [SerializeField] private Color _playerDamageColor = new Color(1f, 0.4f, 0.4f);

        private readonly Queue<GameObject> _pool = new();

        private void Awake()
        {
            if (_container == null)
                _container = transform;

            for (int i = 0; i < _poolSize; i++)
            {
                var go = CreateDamageText();
                go.SetActive(false);
                _pool.Enqueue(go);
            }
        }

        public void ShowDamage(int damage, Vector2 position, bool isCritical = false, bool isHeal = false, bool isPlayerDamage = false)
        {
            var text = GetText();
            text.transform.localPosition = position;
            text.SetActive(true);

            var textComponent = text.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = isHeal ? $"+{damage}" : $"-{damage}";
                textComponent.color = GetColor(isCritical, isHeal, isPlayerDamage);
            }

            StartCoroutine(AnimateDamageText(text, position));
        }

        private IEnumerator AnimateDamageText(GameObject text, Vector2 startPos)
        {
            var textComponent = text.GetComponent<Text>();
            var rectTransform = text.GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                ReturnText(text);
                yield break;
            }

            Vector2 endPos = startPos + Vector2.up * _moveUpDistance;
            float elapsed = 0f;

            Color startColor = textComponent != null ? textComponent.color : Color.white;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < _animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _animationDuration;

                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                if (textComponent != null && elapsed > _animationDuration - _fadeDuration)
                {
                    float fadeT = (elapsed - (_animationDuration - _fadeDuration)) / _fadeDuration;
                    textComponent.color = Color.Lerp(startColor, endColor, fadeT);
                }

                yield return null;
            }

            ReturnText(text);
        }

        private GameObject GetText()
        {
            if (_pool.Count > 0)
            {
                var go = _pool.Dequeue();
                if (go != null) return go;
            }
            return CreateDamageText();
        }

        private void ReturnText(GameObject text)
        {
            if (text == null) return;
            text.SetActive(false);
            _pool.Enqueue(text);
        }

        private GameObject CreateDamageText()
        {
            if (_damageTextPrefab != null)
            {
                return Instantiate(_damageTextPrefab, _container);
            }

            var go = new GameObject("DamageText");
            go.transform.SetParent(_container);

            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 50);

            var text = go.AddComponent<Text>();
            text.font = Font.CreateDynamicFontFromOSFont("Arial", _fontSize);
            text.fontSize = _fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);

            return go;
        }

        private Color GetColor(bool isCritical, bool isHeal, bool isPlayerDamage)
        {
            if (isHeal) return _healColor;
            if (isPlayerDamage) return _playerDamageColor;
            if (isCritical) return _criticalColor;
            return _normalColor;
        }
    }
}