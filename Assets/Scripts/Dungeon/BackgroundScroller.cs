using System;
using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _scrollDuration = 0.5f;
        [SerializeField] private float _scaleAmount = 0.1f;
        
        private RectTransform _rectTransform;
        private Vector3 _originalScale;
        
        public event Action OnScrollComplete;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }
        
        public void ScrollToNextRoom(Action onComplete = null)
        {
            StartCoroutine(ScrollCoroutine(onComplete));
        }
        
        private System.Collections.IEnumerator ScrollCoroutine(Action onComplete)
        {
            float elapsed = 0f;
            
            while (elapsed < _scrollDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _scrollDuration;
                
                float scaleOffset = Mathf.Sin(t * Mathf.PI) * _scaleAmount;
                Vector3 newScale = _originalScale;
                newScale.y += scaleOffset;
                _rectTransform.localScale = newScale;
                
                yield return null;
            }
            
            _rectTransform.localScale = _originalScale;
            onComplete?.Invoke();
            OnScrollComplete?.Invoke();
        }
    }
}