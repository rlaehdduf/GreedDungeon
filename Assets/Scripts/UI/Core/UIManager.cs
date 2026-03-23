using System.Collections.Generic;
using UnityEngine;

namespace GreedDungeon.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[UIManager]");
                        _instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private readonly Dictionary<string, UIView> _views = new();
        private readonly Stack<UIView> _history = new();

        [Header("UI Views")]
        [SerializeField] private UIView[] _registeredViews;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            RegisterViews();
        }

        private void RegisterViews()
        {
            if (_registeredViews == null) return;
            
            foreach (var view in _registeredViews)
            {
                if (view != null)
                {
                    Register(view.GetType().Name, view);
                }
            }
        }

        public void Register<T>(string key, T view) where T : UIView
        {
            _views[key] = view;
        }

        public T Get<T>() where T : UIView
        {
            string key = typeof(T).Name;
            return _views.TryGetValue(key, out var view) ? view as T : null;
        }

        public UIView Get(string key)
        {
            return _views.TryGetValue(key, out var view) ? view : null;
        }

        public void Show<T>() where T : UIView
        {
            var view = Get<T>();
            if (view != null)
            {
                view.Show();
                if (!_history.Contains(view))
                    _history.Push(view);
            }
        }

        public void Hide<T>() where T : UIView
        {
            var view = Get<T>();
            if (view != null)
            {
                view.Hide();
                if (_history.Count > 0 && _history.Peek() == view)
                    _history.Pop();
            }
        }

        public void HideAll()
        {
            foreach (var view in _views.Values)
            {
                view.Hide();
            }
            _history.Clear();
        }

        public void ShowOnly<T>() where T : UIView
        {
            HideAll();
            Show<T>();
        }

        public void Back()
        {
            if (_history.Count > 1)
            {
                _history.Pop();
                _history.Peek().Show();
            }
        }
    }
}