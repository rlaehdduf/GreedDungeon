using System;
using System.Collections;
using UnityEngine;
using GreedDungeon.Character;

namespace GreedDungeon.UI.Battle
{
    public class MonsterSpriteView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        [Header("Breath Animation")]
        [SerializeField] private float _breathSpeed = 2f;
        [SerializeField] private float _breathAmplitude = 0.03f;
        
        [Header("Damage Animation")]
        [SerializeField] private float _damageScaleAmount = 0.9f;
        [SerializeField] private float _damageDuration = 0.3f;
        [SerializeField] private Color _damageColor = new Color(1f, 0.3f, 0.3f);

        [Header("Attack Animation")]
        [SerializeField] private float _attackScale = 1.4f;
        [SerializeField] private float _attackDuration = 0.4f;
        
        private Vector3 _baseScale = Vector3.one;
        private float _breathTimer;
        private bool _isAnimatingDamage;
        private bool _isAnimatingAttack;
        private bool _isSetupComplete;
        private Coroutine _damageCoroutine;
        private Coroutine _attackCoroutine;

        public event Action OnDamageAnimationComplete;

        private void Update()
        {
            if (!_isAnimatingDamage && !_isAnimatingAttack && _isSetupComplete)
            {
                UpdateBreathAnimation();
            }
        }

        private void UpdateBreathAnimation()
        {
            _breathTimer += Time.deltaTime * _breathSpeed;
            float scaleY = _baseScale.y + Mathf.Sin(_breathTimer) * _breathAmplitude;
            transform.localScale = new Vector3(_baseScale.x, scaleY, _baseScale.z);
        }

        public void Setup(Sprite sprite, float scaleX, float scaleY)
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (_spriteRenderer != null)
                _spriteRenderer.sprite = sprite;
            
            _baseScale = new Vector3(scaleX, scaleY, 1f);
            transform.localScale = _baseScale;
            _breathTimer = 0f;
            _isSetupComplete = true;
            Debug.Log($"[MonsterSpriteView] Setup 완료 - Scale: {_baseScale}");
        }

        public void PlayDamageAnimation()
        {
            if (_damageCoroutine != null)
                StopCoroutine(_damageCoroutine);
            
            _damageCoroutine = StartCoroutine(DamageAnimationRoutine());
        }

        public Coroutine PlayAttackAnimation()
        {
            if (_attackCoroutine != null)
                StopCoroutine(_attackCoroutine);
            
            _attackCoroutine = StartCoroutine(AttackAnimationRoutine());
            return _attackCoroutine;
        }

        private IEnumerator DamageAnimationRoutine()
        {
            _isAnimatingDamage = true;
            
            Vector3 originalScale = transform.localScale;
            Color originalColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;
            
            float elapsed = 0f;
            float halfDuration = _damageDuration * 0.5f;
            
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                transform.localScale = Vector3.Lerp(originalScale, originalScale * _damageScaleAmount, t);
                if (_spriteRenderer != null)
                    _spriteRenderer.color = Color.Lerp(originalColor, _damageColor, t);
                
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                
                transform.localScale = Vector3.Lerp(originalScale * _damageScaleAmount, _baseScale, t);
                if (_spriteRenderer != null)
                    _spriteRenderer.color = Color.Lerp(_damageColor, originalColor, t);
                
                yield return null;
            }
            
            transform.localScale = _baseScale;
            if (_spriteRenderer != null)
                _spriteRenderer.color = originalColor;
            
            _isAnimatingDamage = false;
            _damageCoroutine = null;
            OnDamageAnimationComplete?.Invoke();
        }

        private IEnumerator AttackAnimationRoutine()
        {
            _isAnimatingAttack = true;

            Vector3 originalScale = _baseScale;
            Vector3 targetScale = _baseScale * _attackScale;

            float elapsed = 0f;
            float halfDuration = _attackDuration / 2f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = _baseScale;
            _isAnimatingAttack = false;
            _attackCoroutine = null;
        }

        public void SubscribeToMonster(Monster monster)
        {
            if (monster != null)
                monster.OnDamaged += OnMonsterDamaged;
        }

        public void UnsubscribeFromMonster(Monster monster)
        {
            if (monster != null)
                monster.OnDamaged -= OnMonsterDamaged;
        }

        private void OnMonsterDamaged(int damage)
        {
            PlayDamageAnimation();
        }

        private void OnDestroy()
        {
            if (_damageCoroutine != null)
                StopCoroutine(_damageCoroutine);
        }
    }
}