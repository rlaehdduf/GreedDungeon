using System;
using UnityEngine;

namespace GreedDungeon.Dungeon
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _scrollDuration = 2f;
        [SerializeField] private bool _useZoom = true;
        [SerializeField] private float _zoomSpeed = 0.08f;
        [SerializeField] private int _bobCycles = 3;
        [SerializeField] private float _bobAmount = 0.15f;
        [SerializeField] private float _microBobAmount = 0.05f;
        
        private Transform _transform;
        private Vector3 _originalScale;
        private Vector3 _originalPos;
        private bool _isScrolling;
        
        public event Action OnScrollComplete;
        
        private void Awake()
        {
            _transform = transform;
            _originalScale = _transform.localScale;
            _originalPos = _transform.position;
        }
        
        public void ScrollToNextRoom(Action onComplete = null)
        {
            if (_isScrolling) return;
            StartCoroutine(ScrollCoroutine(onComplete));
        }
        
        private System.Collections.IEnumerator ScrollCoroutine(Action onComplete)
        {
            _isScrolling = true;
            float elapsed = 0f;
            
            while (elapsed < _scrollDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _scrollDuration);
                
                float easeT = t * t * (3f - 2f * t);
                
                if (_useZoom)
                {
                    Vector3 newScale = _originalScale * (1f + easeT * _zoomSpeed);
                    _transform.localScale = newScale;
                }
                
                float cycleCount = _bobCycles;
                float cycleT = t * cycleCount * Mathf.PI * 2f;
                float bob = Mathf.Sin(cycleT) * _bobAmount * (1f - easeT * 0.5f);
                float microBob = Mathf.Sin(cycleT * 3f) * _microBobAmount * (1f - easeT * 0.5f);
                
                _transform.position = new Vector3(
                    _originalPos.x,
                    _originalPos.y + bob + microBob,
                    _originalPos.z
                );
                
                yield return null;
            }
            
            if (_useZoom)
                _transform.localScale = _originalScale;
            _transform.position = _originalPos;
            _isScrolling = false;
            onComplete?.Invoke();
            OnScrollComplete?.Invoke();
        }
    }
}