using System;
using System.Collections;
using System.Threading.Tasks;
using GreedDungeon.Character;
using GreedDungeon.Core;
using GreedDungeon.ScriptableObjects;
using UnityEngine;

namespace GreedDungeon.UI.Battle
{
    public class MonsterDisplay : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MonsterSpriteView _spriteView;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private IAssetLoader _assetLoader;
        private Monster _currentMonster;

        private void Awake()
        {
            if (_spriteView == null)
                _spriteView = GetComponent<MonsterSpriteView>();
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            if (Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }
        }

        public async void DisplayMonster(Monster monster)
        {
            if (monster == null) return;

            _currentMonster = monster;
            var data = monster.Data;

            if (_assetLoader == null && Services.IsInitialized)
            {
                _assetLoader = Services.Get<IAssetLoader>();
            }

            if (_assetLoader != null && !string.IsNullOrEmpty(data.PrefabAddress))
            {
                var sprite = await _assetLoader.LoadAssetAsync<Sprite>(data.PrefabAddress);
                if (sprite != null && _spriteView != null)
                {
                    _spriteView.Setup(sprite, data.ScaleX, data.ScaleY);
                }
                else if (sprite != null && _spriteRenderer != null)
                {
                    _spriteRenderer.sprite = sprite;
                    _spriteRenderer.transform.localScale = new Vector3(data.ScaleX, data.ScaleY, 1f);
                }
            }
        }

        public void PlayDamageAnimation()
        {
            if (_spriteView != null)
            {
                _spriteView.PlayDamageAnimation();
            }
        }

        public Coroutine PlayAttackAnimation()
        {
            if (_spriteView != null)
            {
                return _spriteView.PlayAttackAnimation();
            }
            return null;
        }

        public void Clear()
        {
            _currentMonster = null;
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = null;
        }
    }
}