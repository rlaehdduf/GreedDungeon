using UnityEngine;

namespace GreedDungeon.UI
{
    public abstract class UIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public bool IsOpen => gameObject.activeSelf;
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }
        
        public virtual void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }
        
        public virtual void Toggle()
        {
            if (IsOpen) Hide();
            else Show();
        }
    }
}