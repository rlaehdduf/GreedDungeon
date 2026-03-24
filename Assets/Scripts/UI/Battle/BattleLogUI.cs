using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public enum LogType
    {
        Player,
        Monster,
        System
    }

    public class BattleLogUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform _logContainer;
        [SerializeField] private GameObject _logEntryPrefab;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _maxLogCount = 50;

        [Header("Colors")]
        [SerializeField] private Color _playerColor = new Color(0.4f, 0.8f, 1f);
        [SerializeField] private Color _monsterColor = new Color(1f, 0.5f, 0.3f);
        [SerializeField] private Color _systemColor = new Color(1f, 1f, 0.5f);

        private readonly List<GameObject> _logEntries = new();

        public void AddLog(string message, LogType logType = LogType.System)
        {
            if (_logContainer == null || _logEntryPrefab == null) return;

            var go = Instantiate(_logEntryPrefab, _logContainer);
            var text = go.GetComponent<Text>();
            if (text != null)
            {
                text.text = FormatMessage(message, logType);
                text.alignment = logType == LogType.Player ? TextAnchor.MiddleLeft 
                              : logType == LogType.Monster ? TextAnchor.MiddleRight 
                              : TextAnchor.MiddleCenter;
                text.color = GetColor(logType);
            }

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

        private string FormatMessage(string message, LogType logType)
        {
            return logType switch
            {
                LogType.Player => $"◀ {message}",
                LogType.Monster => $"{message} ▶",
                _ => message
            };
        }

        private Color GetColor(LogType logType)
        {
            return logType switch
            {
                LogType.Player => _playerColor,
                LogType.Monster => _monsterColor,
                _ => _systemColor
            };
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