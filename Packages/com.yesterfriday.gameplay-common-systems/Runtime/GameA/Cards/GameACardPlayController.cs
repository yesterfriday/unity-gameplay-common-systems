using UnityEngine;
using Yesterfriday.GameplayCommonSystems.Samples.Common.Gameplay;
using Yesterfriday.GameplayCommonSystems.Samples.GameA.Targeting;

namespace Yesterfriday.GameplayCommonSystems.Samples.GameA.Cards
{
    public sealed class GameACardPlayController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private TargetingController2D _targeting;
        [SerializeField] private Health _playerHealth;

        [Header("Config")]
        [SerializeField] private int _strikeDamage = 3;
        [SerializeField] private int _healAmount = 2;

        private bool _awaitingStrikeTarget;

        private void OnEnable()
        {
            if (_targeting != null)
            {
                _targeting.TargetChanged += OnTargetChanged;
            }
        }

        private void OnDisable()
        {
            if (_targeting != null)
            {
                _targeting.TargetChanged -= OnTargetChanged;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartStrike();

            if (Input.GetKeyDown(KeyCode.Alpha2))
                PlayHeal();

            // ✅ 우클릭으로 모드 종료되면 Strike 대기도 같이 취소(원하는 UX)
            if (_awaitingStrikeTarget && _targeting != null && !_targeting.IsTargeting)
            {
                _awaitingStrikeTarget = false;
                Debug.Log("[Card] Strike canceled (targeting ended).");
            }
        }

        private void StartStrike()
        {
            if (_targeting == null)
            {
                Debug.LogWarning("[Card] TargetingController2D is missing.", this);
                return;
            }

            _awaitingStrikeTarget = true;
            _targeting.BeginTargeting();
            Debug.Log("[Card] Strike: click an enemy (Esc clears, RMB ends).");
        }

        private void PlayHeal()
        {
            if (_playerHealth == null)
            {
                Debug.LogWarning("[Card] Player Health is missing.", this);
                return;
            }

            bool applied = _playerHealth.TryHeal(_healAmount);
            Debug.Log(applied
                ? $"[Card] Heal +{_healAmount} => {_playerHealth.Current}/{_playerHealth.Max}"
                : $"[Card] Heal failed => {_playerHealth.Current}/{_playerHealth.Max}");
        }

        private void OnTargetChanged(Targetable2D target)
        {
            if (!_awaitingStrikeTarget) return;
            if (target == null) return;

            var enemyHealth = target.GetComponentInChildren<Health>();
            if (enemyHealth == null)
            {
                Debug.LogWarning($"[Card] Strike: No Health found under target {target.name}.", target);
                return;
            }

            bool applied = enemyHealth.TryDamage(_strikeDamage);
            Debug.Log(applied
                ? $"[Card] Strike -{_strikeDamage} => {enemyHealth.Current}/{enemyHealth.Max}"
                : $"[Card] Strike failed => {enemyHealth.Current}/{enemyHealth.Max}");

            if (applied)
            {
                _awaitingStrikeTarget = false;
                _targeting.ClearSelection();
            }
        }
    }
}