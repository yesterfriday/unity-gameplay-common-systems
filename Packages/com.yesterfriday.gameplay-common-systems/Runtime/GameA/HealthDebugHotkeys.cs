using UnityEngine;
using Yesterfriday.GameplayCommonSystems.Samples.Common.Gameplay;

public sealed class HealthDebugHotkeys : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private int _dmg = 3;
    [SerializeField] private int _heal = 2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Debug.Log($"TryDamage({_dmg}) => {_health.TryDamage(_dmg)}, Current={_health.Current}/{_health.Max}");

        if (Input.GetKeyDown(KeyCode.W))
            Debug.Log($"TryHeal({_heal}) => {_health.TryHeal(_heal)}, Current={_health.Current}/{_health.Max}");
    }
}