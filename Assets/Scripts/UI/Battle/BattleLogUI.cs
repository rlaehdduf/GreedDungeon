using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class BattleLogUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform _logContainer;
        [SerializeField] private GameObject _logEntryPrefab;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _maxLogCount = 50;

        private readonly List<GameObject> _logEntries = new();

        public void AddLog(string message)
        {
            if (_logContainer == null || _logEntryPrefab == null) return;

            var go = Instantiate(_logEntryPrefab, _logContainer);
            var text = go.GetComponent<Text>();
            if (text != null)
                text.text = message;

            _logEntries.Add(go);

            while (_logEntries.Count > _maxLogCount)
            {
                if (_logEntries[0] != null)
                    Destroy(_logEntries[0]);
                _logEntries.RemoveAt(0);
            }

            if (_scrollRect != null)
                Canvas.ForceUpdateCanvases();
            
            if (_scrollRect != null)
                _scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            foreach (var go in _logEntries)
            {
                if (go != null) Destroy(go);
            }
            _logEntries.Clear();
        }
    }
}