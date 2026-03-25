using UnityEngine;
using UnityEngine.UI;

namespace GreedDungeon.UI.Battle
{
    public class StatusEffectSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Text _countText;

        public void SetIcon(Sprite sprite, int duration)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = sprite;
                _iconImage.enabled = sprite != null;
            }
            SetDuration(duration);
        }

        public void SetDuration(int duration)
        {
            if (_countText != null)
            {
                _countText.text = duration > 0 ? duration.ToString() : "";
            }
        }

        public void Clear()
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
            }
            if (_countText != null)
            {
                _countText.text = "";
            }
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
            if (_countText != null)
            {
                _countText.text = "";
            }
            gameObject.SetActive(false);
        }

        public void PrepareShow()
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
            if (_countText != null)
            {
                _countText.text = "";
            }
            gameObject.SetActive(true);
        }
    }
}