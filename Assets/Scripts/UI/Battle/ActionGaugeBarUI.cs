using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GreedDungeon.Character;

namespace GreedDungeon.UI.Battle
{
    public class ActionGaugeBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _barContainer;
        [SerializeField] private GameObject _markerPrefab;
        [SerializeField] private Sprite _playerMarkerSprite;
        [SerializeField] private Sprite _monsterMarkerSprite;
        [SerializeField] private Color _playerColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _monsterColor = new Color(1f, 0.3f, 0.3f);
        
        private const int GAUGE_THRESHOLD = 1000;
        
        private Dictionary<IBattleEntity, GameObject> _markers = new();
        private float _barWidth;
        
        private void Awake()
        {
            if (_barContainer != null)
            {
                _barWidth = _barContainer.rect.width;
            }
        }
        
        public void Initialize(IEnumerable<IBattleEntity> entities)
        {
            ClearMarkers();
            
            if (_barContainer == null || _markerPrefab == null) return;
            
            foreach (var entity in entities)
            {
                if (entity.IsDead) continue;
                
                var marker = Instantiate(_markerPrefab, _barContainer);
                marker.name = $"Marker_{entity.Name}";
                
                var rectTransform = marker.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(0, 0);
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                }
                
                var image = marker.GetComponent<Image>();
                if (image != null)
                {
                    bool isPlayer = entity is Player;
                    image.sprite = isPlayer ? _playerMarkerSprite : _monsterMarkerSprite;
                    image.color = isPlayer ? _playerColor : _monsterColor;
                }
                
                _markers[entity] = marker;
            }
            
            UpdateAllPositions(entities);
        }
        
        public void UpdateAllPositions(IEnumerable<IBattleEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.IsDead)
                {
                    if (_markers.TryGetValue(entity, out var marker))
                    {
                        marker.SetActive(false);
                    }
                    continue;
                }
                
                UpdateMarkerPosition(entity);
            }
        }
        
        public void UpdateMarkerPosition(IBattleEntity entity)
        {
            if (!_markers.TryGetValue(entity, out var marker)) return;
            if (marker == null) return;
            
            var rectTransform = marker.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            float ratio = Mathf.Clamp01((float)entity.ActionGauge / GAUGE_THRESHOLD);
            float xPos = ratio * _barWidth;
            
            rectTransform.anchoredPosition = new Vector2(xPos, 0);
            
            marker.SetActive(!entity.IsDead);
        }
        
        public void HighlightCurrentEntity(IBattleEntity currentEntity)
        {
            foreach (var kvp in _markers)
            {
                var image = kvp.Value.GetComponent<Image>();
                if (image != null)
                {
                    bool isCurrent = kvp.Key == currentEntity;
                    float scale = isCurrent ? 1.3f : 1f;
                    kvp.Value.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }
        }
        
        public void ClearMarkers()
        {
            foreach (var marker in _markers.Values)
            {
                if (marker != null)
                    Destroy(marker);
            }
            _markers.Clear();
        }
        
        public void RemoveEntity(IBattleEntity entity)
        {
            if (_markers.TryGetValue(entity, out var marker))
            {
                if (marker != null)
                    Destroy(marker);
                _markers.Remove(entity);
            }
        }
    }
}